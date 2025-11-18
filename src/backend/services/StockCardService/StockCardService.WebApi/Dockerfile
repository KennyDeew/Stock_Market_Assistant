# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Путь относительно корня контекста (будем вызывать docker build из корня)
COPY src/backend/services/StockCardService/StockCardService.WebApi/StockCardService.WebApi.csproj src/backend/services/StockCardService/StockCardService.WebApi/
COPY src/backend/services/StockCardService/StockCardService.Application/StockCardService.Application.csproj src/backend/services/StockCardService/StockCardService.Application/
COPY src/backend/services/StockCardService/StockCardService.Domain/StockCardService.Domain.csproj src/backend/services/StockCardService/StockCardService.Domain/
COPY src/backend/services/StockCardService/StockCardService.Infrastructure.EntityFramework/StockCardService.Infrastructure.EntityFramework.csproj src/backend/services/StockCardService/StockCardService.Infrastructure.EntityFramework/

# Восстанавливаем зависимости
RUN dotnet restore "src/backend/services/StockCardService/StockCardService.WebApi/StockCardService.WebApi.csproj"

# Копируем всё остальное
COPY . .

WORKDIR "/src/src/backend/services/StockCardService/StockCardService.WebApi"
RUN dotnet build "StockCardService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "StockCardService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StockCardService.WebApi.dll"]