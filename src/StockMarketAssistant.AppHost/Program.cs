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
        var apiNotificationService = builder.AddProject<Projects.NotificationService>("notificationservice-api");

        // Добавление ресурсов
        var redis = builder.AddRedis("cache");
        var pgPortfolioDb = builder.AddPostgres("pg-portfolio-db")
            //.WithPgAdmin()
            .WithImage("postgres:17.5")
            .WithDataVolume("portfolio-pg-data")
            .WithHostPort(14050)
            .AddDatabase("portfolio-db");
        var pgStockCardDb = builder.AddPostgres("pg-stock-card-db")
            .WithImage("postgres:17.5")
            .WithHostPort(14051)
            .AddDatabase("stock-card-db");

        var notificationPostgres = builder.AddPostgres("notification-db")
            .WithImage("postgres:17.5")
            .WithHostPort(14053).WithPgWeb(n => n.WithHostPort(5000))
            .AddDatabase("notificationDb");

        var kafka = builder.AddKafka("kafka")
            .WithKafkaUI(kafka => kafka.WithHostPort(9100));


        //var postgres = builder.AddPostgres("postgres").AddDatabase("stockcarddb");
        //var container = builder.AddDockerfile("gateway", "../backend/gateway/");

        // Связывание ресурсов с проектами
        apiPortfolioService.WithReference(redis)
                           .WithReference(pgPortfolioDb)
                           .WaitFor(pgPortfolioDb);

        apiStockCardService.WithReference(redis)
                           .WithReference(pgStockCardDb)
                           .WaitFor(pgStockCardDb);

        apiNotificationService
            .WithReference(notificationPostgres)
            .WithReference(kafka)
            .WaitFor(notificationPostgres)
            .WaitFor(kafka);


        builder.Build().Run();
    }
}