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
estar desplegada, ej. Render.com), y ese backend es el único que conoce la key de TMDb.

## 0. IMPORTANTE: no uses una carpeta sincronizada por OneDrive/Google Drive/Dropbox

Si el proyecto vive dentro de una carpeta sincronizada (ej. `OneDrive\Documentos\...`), vas a
tener errores de build intermitentes tipo `CS0041: Access to the path '...pdb' is denied` o
`CS2012: Cannot open '...dll' for writing`, porque el cliente de sincronización bloquea archivos
mientras el compilador los escribe (un build de Android genera cientos de archivos por segundo
en `bin`/`obj`). Cloná/mové el repo a una ruta local normal, ej. `C:\Dev\TrackMuvi`.

## 1. Prerrequisitos

- [.NET SDK 9](https://dotnet.microsoft.com/download) (o el que traiga Visual Studio 2022 17.14+)
- Workload de MAUI para Android:
  ```bash
  dotnet workload install maui-mobile
  ```
  (o instalarlo desde el Visual Studio Installer, componente ".NET Multi-platform App UI")
- **Virtualización de hardware habilitada en la BIOS/UEFI** (Intel VT-x / AMD-V). Sin esto, el
  emulador Android arranca sin aceleración (lentísimo o se cuelga) y Visual Studio puede mostrar
  builds que "se cancelan solos". Comprobalo con:
  ```powershell
  Get-ComputerInfo -Property HyperVRequirementVirtualizationFirmwareEnabled
  ```
  Si da `False`, entrá a la BIOS (tecla Supr/F2 al arrancar) y activá VT-x/AMD-V, y activá también
  "Plataforma de máquina virtual" + "Plataforma de hipervisor de Windows" en "Activar o desactivar
  características de Windows".
- Un emulador Android (API 24 / Android 7.0 o superior) — se crea desde Android Device Manager
  en Visual Studio, o desde Android Studio. También podés correr en un teléfono físico con
  "Depuración USB" activada (Ajustes → Acerca del teléfono → tocar 7 veces "Número de compilación"
  → Opciones de desarrollador → Depuración USB).
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

### Desplegado en Render.com (gratis, sin tarjeta, recomendado)

El repo ya trae [`src/TrackMuvi.Api/Dockerfile`](src/TrackMuvi.Api/Dockerfile) listo para esto.

1. Subí el repo a GitHub (Render despliega desde un repo Git, no acepta subir código a mano).
2. En https://render.com → **Sign up with GitHub** → **New +** → **Web Service**.
3. Conectá tu repo. Configuración del servicio:
   - **Root Directory**: vacío (raíz del repo)
   - **Runtime**: **Docker**
   - **Dockerfile Path**: `src/TrackMuvi.Api/Dockerfile`
   - **Instance Type**: **Free**
4. **Environment Variables** → agregá `Tmdb__AccessToken` = tu token (nunca en appsettings.json).
5. **Create Web Service**. Al terminar el build te da la URL pública (`https://TU-SERVICIO.onrender.com`).

> Nota: `Tmdb__AccessToken` (con doble guion bajo) es la convención de ASP.NET Core para
> mapear variables de entorno a `Tmdb:AccessToken` en configuración anidada.

> El tier gratis de Render "duerme" el servicio tras ~15 min sin tráfico; el primer request
> después de eso tarda 30-60s en responder mientras arranca. Es normal, no es un bug de la app.

### Alternativa: Azure App Service

Si preferís Azure (requiere cuenta con tarjeta, aunque el plan F1 no cobra):

```bash
az group create --name trackmuvi-rg --location eastus
az appservice plan create --name trackmuvi-plan --resource-group trackmuvi-rg --sku F1 --is-linux
az webapp create --name trackmuvi-api --resource-group trackmuvi-rg --plan trackmuvi-plan --runtime "DOTNETCORE:9.0"
az webapp config appsettings set --name trackmuvi-api --resource-group trackmuvi-rg \
  --settings Tmdb__AccessToken="TU_TOKEN_AQUI"
cd src/TrackMuvi.Api
dotnet publish -c Release -o ./publish
az webapp deploy --name trackmuvi-api --resource-group trackmuvi-rg --src-path ./publish --type zip
```

## 4. Configurar el cliente MAUI

Editá [`src/TrackMuvi.Maui/Resources/Raw/appsettings.json`](src/TrackMuvi.Maui/Resources/Raw/appsettings.json):

```json
{
  "Api": {
    "BaseUrl": "https://tu-servicio.onrender.com"
  }
}
```

Con HTTPS (Render/Azure) no hace falta nada más. Si en cambio corrés la API local (`dotnet run`)
para probar más rápido sin desplegar nada:

- **Emulador Android**: usá `http://10.0.2.2:PUERTO` (`10.0.2.2` es el alias del emulador hacia
  el localhost de tu PC).
- **Teléfono físico**: el teléfono y la PC tienen que estar en la **misma WiFi**. Usá la IP de
  tu PC en esa red (`ipconfig` → adaptador Wi-Fi), ej. `http://192.168.1.50:PUERTO`, y corré la
  API con `dotnet run --urls http://0.0.0.0:PUERTO` (si solo escucha en `localhost`, el teléfono
  no la va a poder alcanzar aunque esté en la misma red). También necesitás:
  1. Una regla de firewall que deje pasar ese puerto (PowerShell como Administrador):
     ```powershell
     New-NetFirewallRule -DisplayName "TrackMuvi API (dev)" -Direction Inbound -Protocol TCP -LocalPort PUERTO -Action Allow
     ```
  2. Que el teléfono **no** tenga el tethering/anclaje USB activado — muchos Android apagan el
     WiFi automáticamente al activar el tethering, y ahí el teléfono deja de estar en tu red WiFi.

Como HTTP sin cifrar está bloqueado por Android desde API 28, ambos casos (`10.0.2.2` y la IP de
tu WiFi) están declarados como excepción de cleartext en
[`Platforms/Android/Resources/xml/network_security_config.xml`](src/TrackMuvi.Maui/Platforms/Android/Resources/xml/network_security_config.xml).
Si tu IP de WiFi es distinta, agregá tu propia IP ahí (o volvé a HTTPS con Render/Azure, que no
necesita esta excepción).

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

## Problemas comunes al correr esto por primera vez

- **La app se instala pero se cierra sola con `Java.Lang.IllegalArgumentException: No view
  found for id ... NavigationRootManager_ElementBasedFragment`**: bug conocido de .NET MAUI en
  Android, típicamente por una instalación previa parcial con recursos desincronizados (pasa
  seguido en el primer deploy a un dispositivo nuevo). Se arregla desinstalando la app del
  dispositivo (`adb uninstall com.trackmuvi.app`) y volviendo a compilar/desplegar limpio
  (borrando `bin`/`obj` de `TrackMuvi.Maui` si persiste).
- **Al correr la API local, el puerto por defecto (5037 en algunos perfiles) da "address already
  in use"**: es el puerto que usa `adb` (el puente de depuración de Android), no tiene nada que
  ver con .NET. Usá otro puerto, ej. `dotnet run --urls http://0.0.0.0:5180`.
- **Visual Studio no detecta tu teléfono aunque `adb devices` sí lo liste como `device`**:
  reiniciá el servidor de adb (`adb kill-server && adb start-server`) y cerrá/reabrí Visual
  Studio por completo (verificá que no quede `devenv.exe` en el Administrador de tareas).
- **La build "se cancela" sola en Visual Studio sin error real**: normalmente es la
  virtualización de hardware apagada (ver sección de Prerrequisitos) haciendo que el emulador
  tarde muchísimo o se cuelgue, y VS termina cancelando el deploy.

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
