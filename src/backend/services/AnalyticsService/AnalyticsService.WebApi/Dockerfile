# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Создаем директорию для секретов
RUN mkdir -p /run/secrets && chmod 700 /run/secrets

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/backend/services/AnalyticsService/AnalyticsService.WebApi/AnalyticsService.WebApi.csproj", "src/backend/services/AnalyticsService/AnalyticsService.WebApi/"]
RUN dotnet restore "./src/backend/services/AnalyticsService/AnalyticsService.WebApi/AnalyticsService.WebApi.csproj"
COPY . .
WORKDIR "/src/src/backend/services/AnalyticsService/AnalyticsService.WebApi"
RUN dotnet build "./AnalyticsService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AnalyticsService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Устанавливаем права на секреты
RUN chown -R $APP_UID:$APP_UID /run/secrets

# Переменные окружения по умолчанию
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AnalyticsService.WebApi.dll"]