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
        var pgStockCardDb = builder.AddPostgres("pg-stock-card-db")
            .WithImage("postgres:17.5")
            .WithHostPort(14051)
            .AddDatabase("stock-card-db");
        var mongo = builder.AddMongoDB("mongo")
            .WithImage("mongo:7.0")
            .WithDataVolume("stock-mongo-data").WithEndpoint("mongodb", endpoint =>
            {
                endpoint.Port = 14052;       // внешний порт на хосте
                endpoint.TargetPort = 27017; // внутренний порт контейнера MongoDB
            });
        var mongoStockCardDb = mongo.AddDatabase("finantial-report-db");

        //var container = builder.AddDockerfile("gateway", "../backend/gateway/");

        // Связывание ресурсов с проектами
        apiPortfolioService.WithReference(redis)
                           .WithReference(pgPortfolioDb)
                           .WaitFor(pgPortfolioDb);

        apiStockCardService.WithReference(redis)
                           .WithReference(pgStockCardDb)
                           .WithReference(mongoStockCardDb)
                           .WaitFor(pgStockCardDb);


        builder.Build().Run();
    }
}