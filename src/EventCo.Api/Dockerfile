# Frontend : buildé à part (same-origin en prod, cf. AuthController/vite.config.ts/api.ts) puis
# copié dans wwwroot de l'API ci-dessous, sans VITE_API_URL pour garder les appels en relatif.
FROM node:22-alpine AS client-build
WORKDIR /client
COPY client/package.json client/package-lock.json ./
RUN npm ci
COPY client/ ./
# Version du build (SHA git), partagée avec l'API ci-dessous : le frontend se recharge s'il reçoit de l'API
# une version différente de la sienne (cf. client/src/shared/lib/appVersion.ts). APP_VERSION explicite en
# priorité, sinon RENDER_GIT_COMMIT (build arg transmis par Render), sinon "dev" (détection désactivée).
# Déclarés après `npm ci` pour ne pas invalider le cache des dépendances à chaque commit.
ARG RENDER_GIT_COMMIT
ARG APP_VERSION=${RENDER_GIT_COMMIT:-dev}
ENV VITE_APP_VERSION=${APP_VERSION}
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/EventCo.Api/EventCo.Api.csproj", "src/EventCo.Api/"]
COPY ["src/EventCo.Application/EventCo.Application.csproj", "src/EventCo.Application/"]
COPY ["src/EventCo.Infrastructure/EventCo.Infrastructure.csproj", "src/EventCo.Infrastructure/"]
COPY ["src/EventCo.Domain/EventCo.Domain.csproj", "src/EventCo.Domain/"]
RUN dotnet restore "src/EventCo.Api/EventCo.Api.csproj"

COPY src/ src/
RUN dotnet publish "src/EventCo.Api/EventCo.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
# Npgsql charge libgssapi_krb5.so.2 (GSSAPI/Kerberos), absente des images aspnet récentes ;
# le paquet libgssapi-krb5-2 la fournit et tire libkrb5-3 en dépendance.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=client-build /client/dist ./wwwroot
# Même résolution que pour le frontend (client-build) : les deux doivent toujours porter la même version.
ARG RENDER_GIT_COMMIT
ARG APP_VERSION=${RENDER_GIT_COMMIT:-dev}
ENV App__Version=${APP_VERSION}
EXPOSE 8080
# Render (et d'autres PaaS) injectent PORT et attendent que le service l'écoute ; ASPNETCORE_URLS
# explicite (docker-compose.yml en dev) garde priorité s'il est déjà défini.
ENTRYPOINT ["sh", "-c", "export ASPNETCORE_URLS=${ASPNETCORE_URLS:-http://+:${PORT:-8080}}; exec dotnet EventCo.Api.dll"]