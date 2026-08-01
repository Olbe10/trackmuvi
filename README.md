# TrackMuvi

App Android (MAUI Blazor Hybrid) para descubrir y hacer seguimiento de películas y series,
con calendario de estrenos, notificaciones locales y sincronización con el Calendario nativo
del dispositivo. El catálogo viene de [TMDb](https://www.themoviedb.org/).

## Arquitectura

```
TrackMuvi.sln
├── src/
│   ├── TrackMuvi.Maui/        # App Android (MAUI Blazor Hybrid). Único head compilado en este MVP.
│   ├── TrackMuvi.UI/          # Razor Class Library: toda la UI (Home/Calendar/Discover/MyList/Profile/Detail)
│   ├── TrackMuvi.Shared/      # DTOs y enums compartidos entre API y cliente
│   ├── TrackMuvi.Data/        # EF Core + SQLite (estado personal e historial, local al dispositivo)
│   ├── TrackMuvi.Services/    # Cliente HTTP hacia TrackMuvi.Api + lógica de negocio del cliente
│   └── TrackMuvi.Api/         # ASP.NET Core Web API: proxy hacia TMDb (la API key vive solo acá)
└── tests/TrackMuvi.Tests/     # xUnit
```

La TMDb API key nunca viaja al APK: el cliente MAUI le habla solo a `TrackMuvi.Api` (que debe
estar desplegado, ej. Azure App Service), y ese backend es el único que conoce la key de TMDb.

## 1. Prerrequisitos

- [.NET SDK 9](https://dotnet.microsoft.com/download) (o el que traiga Visual Studio 2022 17.14+)
- Workload de MAUI para Android:
  ```bash
  dotnet workload install maui-mobile
  ```
  (o instalarlo desde el Visual Studio Installer, componente ".NET Multi-platform App UI")
- Un emulador Android (API 24 / Android 7.0 o superior) — se crea desde Android Device Manager
  en Visual Studio, o desde Android Studio.
- Una cuenta gratuita de [TMDb](https://www.themoviedb.org/signup) para la API key.

## 2. Obtener la API key de TMDb (gratis)

1. Crea una cuenta en https://www.themoviedb.org/signup
2. Ve a tu perfil → **Settings** → **API**
3. Solicita una API key tipo **Developer** (uso no comercial, aprobación inmediata)
4. Copia el **API Read Access Token** (el token largo, autenticación v4 — no la "API Key (v3
   auth)" corta). Este proyecto usa ese token como `Bearer` contra la API de TMDb.

## 3. Configurar y desplegar TrackMuvi.Api

El backend necesita el token de TMDb **fuera del repo**.

### En desarrollo local

```bash
cd src/TrackMuvi.Api
dotnet user-secrets set "Tmdb:AccessToken" "TU_TOKEN_AQUI"
dotnet run
```

Esto levanta la API en `http://localhost:5xxx` (revisa la consola). Podés probar los
endpoints en `/openapi/v1.json` o con el archivo `.http` si usás VS Code/Rider.

### Desplegado en Azure App Service (recomendado para probar desde el emulador)

```bash
# Crear el recurso (ajustá nombre/región/plan a lo que ya tengas disponible)
az group create --name trackmuvi-rg --location eastus
az appservice plan create --name trackmuvi-plan --resource-group trackmuvi-rg --sku F1 --is-linux
az webapp create --name trackmuvi-api --resource-group trackmuvi-rg --plan trackmuvi-plan --runtime "DOTNETCORE:9.0"

# Configurar la TMDb key como App Setting (nunca en appsettings.json)
az webapp config appsettings set --name trackmuvi-api --resource-group trackmuvi-rg \
  --settings Tmdb__AccessToken="TU_TOKEN_AQUI"

# Publicar
cd src/TrackMuvi.Api
dotnet publish -c Release -o ./publish
az webapp deploy --name trackmuvi-api --resource-group trackmuvi-rg --src-path ./publish --type zip
```

Anotá la URL pública (`https://trackmuvi-api.azurewebsites.net`) — la necesitás en el paso 4.

> Nota: `Tmdb__AccessToken` (con doble guion bajo) es la convención de ASP.NET Core para
> mapear variables de entorno/App Settings a `Tmdb:AccessToken` en configuración anidada.

## 4. Configurar el cliente MAUI

Editá [`src/TrackMuvi.Maui/Resources/Raw/appsettings.json`](src/TrackMuvi.Maui/Resources/Raw/appsettings.json):

```json
{
  "Api": {
    "BaseUrl": "https://trackmuvi-api.azurewebsites.net"
  }
}
```

Si estás corriendo la API en tu propia máquina (`dotnet run` local) y probando en el
**emulador** Android (no en un dispositivo físico), el emulador ve tu `localhost` como
`10.0.2.2`, por ejemplo `"BaseUrl": "http://10.0.2.2:5xxx"`.

## 5. Compilar y correr en el emulador

### Desde Visual Studio 2022

1. Abrí `TrackMuvi.sln`
2. Seleccioná `TrackMuvi.Maui` como proyecto de inicio
3. Elegí un emulador Android en el selector de dispositivo
4. F5

### Desde línea de comandos

```bash
dotnet build src/TrackMuvi.Maui/TrackMuvi.Maui.csproj -f net9.0-android
dotnet build src/TrackMuvi.Maui/TrackMuvi.Maui.csproj -f net9.0-android -t:Run
```

(el segundo comando instala y abre la app en el emulador/dispositivo conectado)

## 6. Probar la notificación local y la sincronización de calendario

1. Abrí la app → pestaña **Perfil**
2. Botón **"Enviar notificación de prueba"** → aceptá el permiso de notificaciones →
   debería aparecer una notificación del sistema.
3. Marcá algún título como "Quiero ver" o "Siguiendo" desde su ficha (Descubrir → título → botón de estado)
4. Volvé a Perfil → **"Sincronizar con el Calendario del dispositivo"** → aceptá el permiso
   de calendario → revisá la app de Calendario nativa: debería aparecer un evento de todo el
   día en la fecha de estreno de cada título marcado.

## 7. Tests

```bash
dotnet test tests/TrackMuvi.Tests/TrackMuvi.Tests.csproj
```

## Decisiones y límites de este MVP (léelo antes de reportar "bugs" de datos)

- **"Universo" (Marvel/DC/Independiente)** no es un campo real de TMDb: se infiere por nombre
  de productora/colección (`TrackMuvi.Api/Mapping/UniverseMapper.cs`). Es cosmético, no una
  taxonomía oficial.
- El filtro por Marvel/DC/Independiente en **Descubrir** resuelve el universo bajo demanda
  (una llamada de detalle por título visible), porque TMDb no permite filtrar listados por
  productora directamente.
- **"Tus plataformas"** en Descubrir es una grilla fija (Netflix/Max/Prime Video/Disney+), no
  interactiva todavía: personalizarla requeriría guardar preferencias de plataforma por
  usuario, fuera de este MVP.
- La sección "Porque sigues X" (recomendaciones) del mockup **no se implementó**: requeriría
  el endpoint de recomendaciones de TMDb (`/movie|tv/{id}/recommendations`), no solo el catálogo
  de tendencias/estrenos ya cableado. Recomendaciones con IA está fuera de alcance por pedido
  explícito; esto tampoco es IA, pero quedó fuera para no crecer el scope del back sin pedirlo.
- Sin autenticación multiusuario: el perfil es de un solo usuario local (como pedía el MVP).
- Solo Android compila/corre hoy. `TrackMuvi.UI` y `TrackMuvi.Services` ya son multiplataforma;
  agregar iOS solo requeriría (a) descomentar los TargetFrameworks en `TrackMuvi.Maui.csproj`
  y (b) implementar `INotificationService`/`ICalendarSyncService` para iOS en `Platforms/iOS`.
- Notificaciones y chequeo de estrenos/episodios corren al abrir la app (no hay un
  background service del SO todavía) — alcanza para el requisito del MVP de notificaciones
  locales funcionando, pero no dispara notificaciones con la app cerrada.
