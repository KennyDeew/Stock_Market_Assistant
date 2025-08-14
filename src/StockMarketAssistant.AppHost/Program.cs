using System;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        // Добавление проектов

        var apiGatewayService = builder.AddProject<Projects.Gateway_WebApi>("gateway-api");
        var apiAuthService = builder.AddProject<Projects.AuthService_WebApi>("authservice-api");
        var apiStockCardService = builder.AddProject<Projects.StockCardService_WebApi>("stockcardservice-api");
        var apiPortfolioService = builder.AddProject<Projects.PortfolioService_WebApi>("portfolioservice-api");
        var apiAnalyticsService = builder.AddProject<Projects.AnalyticsService_WebApi>("analyticsservice-api");
        var apiNotificationService = builder.AddProject<Projects.NotificationService_WebApi>("notificationservice-api");

        // Добавление ресурсов
        var redis = builder.AddRedis("cache");
        var pgPortfolioDb = builder.AddPostgres("pg-portfolio-db")
            //.WithPgAdmin()
            .WithImage("postgres:17.5")
            .WithDataVolume("portfolio-pg-data")
            .WithHostPort(14050)
            .AddDatabase("portfolio-db");

        var pgAnalyticsDb = builder.AddPostgres("pg-analytics-db")
            .WithImage("postgres:17.5")
            .WithDataVolume("analytics-pg-data")
            .WithHostPort(15432)
            .AddDatabase("analytics-db");

        //var postgres = builder.AddPostgres("postgres").AddDatabase("stockcarddb");
        //var container = builder.AddDockerfile("gateway", "../backend/gateway/");

        // Связывание ресурсов с проектами
        apiPortfolioService.WithReference(redis)
                           .WithReference(pgPortfolioDb)
                           .WaitFor(pgPortfolioDb);

        apiAnalyticsService.WithReference(redis)
                          .WithReference(pgAnalyticsDb)
                          .WaitFor(pgAnalyticsDb);

        builder.Build().Run();
    }
}