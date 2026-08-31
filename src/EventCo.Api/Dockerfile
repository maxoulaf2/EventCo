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
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "EventCo.Api.dll"]
