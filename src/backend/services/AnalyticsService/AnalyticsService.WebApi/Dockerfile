# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/backend/services/AnalyticsService/AnalyticsService.WebApi/AnalyticsService.WebApi.csproj", "src/backend/services/AnalyticsService/AnalyticsService.WebApi/"]
COPY ["src/StockMarketAssistant.ServiceDefaults/StockMarketAssistant.ServiceDefaults.csproj", "src/StockMarketAssistant.ServiceDefaults/"]
COPY ["src/backend/services/AnalyticsService/AnalyticsService.Domain/AnalyticsService.Domain.csproj", "src/backend/services/AnalyticsService/AnalyticsService.Domain/"]
COPY ["src/backend/services/AnalyticsService/AnalyticsService.Application/AnalyticsService.Application.csproj", "src/backend/services/AnalyticsService/AnalyticsService.Application/"]
COPY ["src/backend/services/AnalyticsService/AnalyticsService.Infrastructure.EntityFramework/AnalyticsService.Infrastructure.EntityFramework.csproj", "src/backend/services/AnalyticsService/AnalyticsService.Infrastructure.EntityFramework/"]
COPY ["src/backend/services/AnalyticsService/AnalyticsService.Infrastructure.Security/AnalyticsService.Infrastructure.Security.csproj", "src/backend/services/AnalyticsService/AnalyticsService.Infrastructure.Security/"]
COPY ["src/backend/services/AnalyticsService/AnalyticsService.TestDataGenerator/AnalyticsService.TestDataGenerator.csproj", "src/backend/services/AnalyticsService/AnalyticsService.TestDataGenerator/"]
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
ENTRYPOINT ["dotnet", "AnalyticsService.WebApi.dll"]