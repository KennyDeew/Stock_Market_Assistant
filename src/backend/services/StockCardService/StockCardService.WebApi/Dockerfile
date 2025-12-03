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
COPY ["src/backend/services/StockCardService/StockCardService.WebApi/StockCardService.WebApi.csproj", "src/backend/services/StockCardService/StockCardService.WebApi/"]
COPY ["src/StockMarketAssistant.ServiceDefaults/StockMarketAssistant.ServiceDefaults.csproj", "src/StockMarketAssistant.ServiceDefaults/"]
COPY ["src/backend/services/StockCardService/StockCardService.Application/StockCardService.Application.csproj", "src/backend/services/StockCardService/StockCardService.Application/"]
COPY ["src/backend/services/StockCardService/StockCardService.Abstractions.Repositories/StockCardService.Abstractions.Repositories.csproj", "src/backend/services/StockCardService/StockCardService.Abstractions.Repositories/"]
COPY ["src/backend/services/StockCardService/StockCardService.Domain/StockCardService.Domain.csproj", "src/backend/services/StockCardService/StockCardService.Domain/"]
COPY ["src/backend/services/StockCardService/StockCardService.Infrastructure.Messaging.Kafka/StockCardService.Infrastructure.Messaging.Kafka.csproj", "src/backend/services/StockCardService/StockCardService.Infrastructure.Messaging.Kafka/"]
COPY ["src/backend/services/StockCardService/StockCardService.Infrastructure.EntityFramework/StockCardService.Infrastructure.EntityFramework.csproj", "src/backend/services/StockCardService/StockCardService.Infrastructure.EntityFramework/"]
COPY ["src/backend/services/StockCardService/StockCardService.Infrastructure.Integrations.Moex/StockCardService.Infrastructure.Integrations.Moex.csproj", "src/backend/services/StockCardService/StockCardService.Infrastructure.Integrations.Moex/"]
COPY ["src/backend/services/StockCardService/StockCardService.Infrastructure.MongoDb/StockCardService.Infrastructure.MongoDb.csproj", "src/backend/services/StockCardService/StockCardService.Infrastructure.MongoDb/"]
COPY ["src/backend/services/StockCardService/StockCardService.Infrastructure.Repositories/StockCardService.Infrastructure.Repositories.csproj", "src/backend/services/StockCardService/StockCardService.Infrastructure.Repositories/"]
RUN dotnet restore "./src/backend/services/StockCardService/StockCardService.WebApi/StockCardService.WebApi.csproj"
COPY . .
WORKDIR "/src/src/backend/services/StockCardService/StockCardService.WebApi"
RUN dotnet build "./StockCardService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./StockCardService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StockCardService.WebApi.dll"]