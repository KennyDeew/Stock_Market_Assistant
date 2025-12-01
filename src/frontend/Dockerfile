# Use Node.js LTS (e.g., 20 or 22)
FROM node:20-alpine

# Set working directory
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm install

# Copy source code
COPY . .

# Expose Vite's default dev port
EXPOSE 5173

# Start dev server
CMD ["npm", "run", "dev"]


## См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.
#
## Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
#FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
#USER $APP_UID
#WORKDIR /app
#EXPOSE 8080
#EXPOSE 8081
#
#
## Этот этап используется для сборки проекта службы
#FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["StockMarketAssistant.Frontend.csproj", "."]
#RUN dotnet restore "./StockMarketAssistant.Frontend.csproj"
#COPY . .
#WORKDIR "/src/."
#RUN dotnet build "./StockMarketAssistant.Frontend.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
## Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./StockMarketAssistant.Frontend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
## Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "StockMarketAssistant.Frontend.dll"]