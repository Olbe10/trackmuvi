using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using TrackMuvi.Api.Options;
using TrackMuvi.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();

builder.Services
    .AddOptions<TmdbOptions>()
    .Bind(builder.Configuration.GetSection(TmdbOptions.SectionName))
    .Validate(o => !string.IsNullOrWhiteSpace(o.AccessToken),
        "Falta configurar Tmdb:AccessToken (dotnet user-secrets en desarrollo, App Setting 'Tmdb__AccessToken' en Azure). Ver README.")
    .ValidateOnStart();

builder.Services.AddHttpClient<ITmdbClient, TmdbClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<TmdbOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.AccessToken);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddScoped<GenreCache>();

// El cliente MAUI llama sobre HTTP nativo (no navegador), pero dejamos CORS abierto
// para poder probar la API desde Swagger/un futuro host Blazor WASM.
const string corsPolicy = "TrackMuviClients";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(corsPolicy);
app.UseAuthorization();
app.MapControllers();

app.Run();
