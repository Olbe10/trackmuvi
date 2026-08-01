using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseLocalNotification()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
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

        using (var scope = app.Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<TrackMuviDbContext>().Database.Migrate();
        }

        return app;
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
