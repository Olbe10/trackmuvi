using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Plugin.LocalNotification;
using TrackMuvi.Data;
using TrackMuvi.Maui.Services;
using TrackMuvi.Services;
using TrackMuvi.Services.BackgroundSync;
using TrackMuvi.Services.Calendar;
using TrackMuvi.UI.State;

namespace TrackMuvi.Maui;

public static class MauiProgram
{
    /// <summary>Se completa recién después de builder.Build(); el hook de OnResume (registrado
    /// antes de Build) lo lee más tarde, cuando ya tiene valor.</summary>
    private static IServiceProvider? _services;

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseLocalNotification()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            })
            .ConfigureLifecycleEvents(events =>
            {
                // Cada vez que se vuelve a la app (no solo en el arranque en frío) se corre el
                // chequeo de estrenos/episodios. Es lo más cerca de "siempre al día" que se puede
                // lograr sin un worker nativo en segundo plano: cubre el caso común de abrir la
                // app varias veces por día, no solo la primera vez que se instala.
                events.AddAndroid(android => android.OnResume(activity =>
                {
                    if (_services is { } services) _ = RunReleaseCheckOnceAsync(services);
                }));
            });

        builder.Services.AddMauiBlazorWebView();

        // appsettings.json empaquetado como MauiAsset (Resources/Raw). Ahí vive el BaseUrl
        // de TrackMuvi.Api (nunca la TMDb key: esa solo existe en el backend).
        var apiBaseUrl = ReadApiBaseUrl();

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "trackmuvi.db");

        builder.Services.AddTrackMuviData(dbPath);
        builder.Services.AddTrackMuviServices(apiBaseUrl);
        builder.Services.AddSingleton<AppState>();

        // Implementaciones de plataforma. Hoy solo Android; al agregar iOS/Windows,
        // estas 3 líneas son las que habría que condicionar por plataforma.
        builder.Services.AddSingleton<IKeyValueStore, PreferencesKeyValueStore>();
        builder.Services.AddSingleton<TrackMuvi.Services.Notifications.INotificationService, AndroidNotificationService>();
        builder.Services.AddSingleton<ICalendarSyncService, AndroidCalendarSyncService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        _services = app.Services;

        // La migración (y de ahí en más el chequeo de notificaciones) corren en segundo plano.
        // Antes Database.Migrate() se llamaba de forma síncrona acá mismo, en CreateMauiApp(), que
        // en Android se ejecuta en el hilo principal: si el archivo SQLite quedaba con un lock
        // trabado (pasó en un dispositivo real — logcat mostró "Getting pending lock ... failed"),
        // la app se congelaba lo suficiente para que Android la matara por ANR ("no responde") justo
        // al abrir. Ahora nunca bloquea el hilo principal, pase lo que pase con ese lock.
        _ = InitializeAsync(app.Services);

        return app;
    }

    private static async Task InitializeAsync(IServiceProvider services)
    {
        using (var scope = services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<TrackMuviDbContext>().Database.MigrateAsync();
        }

        // ReleaseCheckService (estrenos mañana, nuevos episodios, cambios de fecha) existía pero
        // nadie lo llamaba. Se corre: al arrancar en frío, cada vez que se vuelve a la app (ver
        // OnResume arriba), y de ahí en más cada 6 horas mientras el proceso siga vivo. Ojo: nada
        // de esto dispara notificaciones con la app cerrada/matada del todo (eso necesitaría un
        // WorkManager nativo aparte); es "best effort" mientras se usa la app con cierta frecuencia.
        await RunReleaseCheckLoopAsync(services);
    }

    private static async Task RunReleaseCheckLoopAsync(IServiceProvider services)
    {
        while (true)
        {
            await RunReleaseCheckOnceAsync(services);
            await Task.Delay(TimeSpan.FromHours(6));
        }
    }

    private static async Task RunReleaseCheckOnceAsync(IServiceProvider services)
    {
        try
        {
            using var scope = services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IReleaseCheckService>().RunCheckAsync();
        }
        catch
        {
            // best effort: un fallo de red/API acá no debe tumbar la app.
        }
    }

    private static string ReadApiBaseUrl()
    {
        using var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult();
        using var doc = JsonDocument.Parse(stream);
        var baseUrl = doc.RootElement.GetProperty("Api").GetProperty("BaseUrl").GetString();
        return string.IsNullOrWhiteSpace(baseUrl)
            ? throw new InvalidOperationException("Falta 'Api:BaseUrl' en Resources/Raw/appsettings.json.")
            : baseUrl;
    }
}
