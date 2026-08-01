# Build desde la raíz del repo (Render: root directory ".", Dockerfile path "src/TrackMuvi.Api/Dockerfile")
# porque TrackMuvi.Api referencia a TrackMuvi.Shared vía ProjectReference.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/TrackMuvi.Api/TrackMuvi.Api.csproj", "src/TrackMuvi.Api/"]
COPY ["src/TrackMuvi.Shared/TrackMuvi.Shared.csproj", "src/TrackMuvi.Shared/"]
RUN dotnet restore "src/TrackMuvi.Api/TrackMuvi.Api.csproj"

COPY src/TrackMuvi.Api/ src/TrackMuvi.Api/
COPY src/TrackMuvi.Shared/ src/TrackMuvi.Shared/
RUN dotnet publish "src/TrackMuvi.Api/TrackMuvi.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render inyecta la variable $PORT en runtime; si no está seteada, cae a 8080 para correr local.
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["/bin/sh", "-c", "exec dotnet TrackMuvi.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
