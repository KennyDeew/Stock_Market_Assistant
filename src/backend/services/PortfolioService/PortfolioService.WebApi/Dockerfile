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
COPY ["src/backend/services/PortfolioService/PortfolioService.WebApi/PortfolioService.WebApi.csproj", "src/backend/services/PortfolioService/PortfolioService.WebApi/"]
COPY ["src/StockMarketAssistant.ServiceDefaults/StockMarketAssistant.ServiceDefaults.csproj", "src/StockMarketAssistant.ServiceDefaults/"]
COPY ["src/backend/services/PortfolioService/PortfolioService.Application/PortfolioService.Application.csproj", "src/backend/services/PortfolioService/PortfolioService.Application/"]
COPY ["src/StockMarketAssistant.SharedLibrary/StockMarketAssistant.SharedLibrary.csproj", "src/StockMarketAssistant.SharedLibrary/"]
COPY ["src/backend/services/PortfolioService/PortfolioService.Domain/PortfolioService.Domain.csproj", "src/backend/services/PortfolioService/PortfolioService.Domain/"]
COPY ["src/backend/services/PortfolioService/PortfolioService.Infrastructure.Caching/PortfolioService.Infrastructure.Caching.csproj", "src/backend/services/PortfolioService/PortfolioService.Infrastructure.Caching/"]
COPY ["src/backend/services/PortfolioService/PortfolioService.Infrastructure.EntityFramework/PortfolioService.Infrastructure.EntityFramework.csproj", "src/backend/services/PortfolioService/PortfolioService.Infrastructure.EntityFramework/"]
COPY ["src/backend/services/PortfolioService/PortfolioService.Infrastructure.Gateways/PortfolioService.Infrastructure.Gateways.csproj", "src/backend/services/PortfolioService/PortfolioService.Infrastructure.Gateways/"]
COPY ["src/backend/services/PortfolioService/PortfolioService.Infrastructure.Repositories/PortfolioService.Infrastructure.Repositories.csproj", "src/backend/services/PortfolioService/PortfolioService.Infrastructure.Repositories/"]
RUN dotnet restore "./src/backend/services/PortfolioService/PortfolioService.WebApi/PortfolioService.WebApi.csproj"
COPY . .
WORKDIR "/src/src/backend/services/PortfolioService/PortfolioService.WebApi"
RUN dotnet build "./PortfolioService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PortfolioService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PortfolioService.WebApi.dll"]