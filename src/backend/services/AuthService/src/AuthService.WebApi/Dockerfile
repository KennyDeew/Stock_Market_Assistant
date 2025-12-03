# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/backend/services/AuthService/src/AuthService.WebApi/AuthService.WebApi.csproj", "src/backend/services/AuthService/src/AuthService.WebApi/"]
COPY ["src/StockMarketAssistant.ServiceDefaults/StockMarketAssistant.ServiceDefaults.csproj", "src/StockMarketAssistant.ServiceDefaults/"]
COPY ["src/backend/services/AuthService/src/AuthService.Domain/AuthService.Domain.csproj", "src/backend/services/AuthService/src/AuthService.Domain/"]
COPY ["src/backend/services/AuthService/src/SharedKernel/SharedKernel.csproj", "src/backend/services/AuthService/src/SharedKernel/"]
COPY ["src/backend/services/AuthService/src/AuthService.Infrastructure.Postgres/AuthService.Infrastructure.Postgres.csproj", "src/backend/services/AuthService/src/AuthService.Infrastructure.Postgres/"]
COPY ["src/backend/services/AuthService/src/AuthService.Application/AuthService.Application.csproj", "src/backend/services/AuthService/src/AuthService.Application/"]
COPY ["src/backend/services/AuthService/src/AuthService.Contracts/AuthService.Contracts.csproj", "src/backend/services/AuthService/src/AuthService.Contracts/"]
COPY ["src/backend/services/AuthService/src/AuthService.Presentation/AuthService.Presentation.csproj", "src/backend/services/AuthService/src/AuthService.Presentation/"]
RUN dotnet restore "./src/backend/services/AuthService/src/AuthService.WebApi/AuthService.WebApi.csproj"
COPY . .
WORKDIR "/src/src/backend/services/AuthService/src/AuthService.WebApi"
RUN dotnet build "./AuthService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AuthService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthService.WebApi.dll"]