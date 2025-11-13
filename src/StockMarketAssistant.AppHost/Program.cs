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
        //var apiAnalyticsService = builder.AddProject<Projects.AnalyticsService_WebApi>("analyticsservice-api");
        var apiNotificationService = builder.AddProject<Projects.NotificationService>("notificationservice-api");

        // Добавление ресурсов
        var redis = builder.AddRedis("cache");

        var kafka = builder.AddKafka("kafka")
            .WithKafkaUI(kafka => kafka.WithHostPort(9100));

        var pgPortfolioDb = builder.AddPostgres("pg-portfolio-db")
            //.WithPgAdmin()
            .WithImage("postgres:17.5")
            .WithDataVolume("portfolio-pg-data")
            .WithHostPort(14050)
            .AddDatabase("portfolio-db");
        var pgAuthDb = builder.AddPostgres("pg-auth-db")
            .WithImage("postgres:17.5")
            .WithDataVolume("auth-pg-data")
            .WithHostPort(14051)
            .AddDatabase("Database");
        var pgStockCardDb = builder.AddPostgres("pg-stock-card-db")
            .WithImage("postgres:17.5")
            .WithHostPort(14052)
            .WithPgWeb(n => n.WithHostPort(5001))
            .AddDatabase("stock-card-db");
        var mongoStockCardDb = builder.AddMongoDB("mongo-stock-card-db", 14053)
            //.WithLifetime(ContainerLifetime.Persistent)
            .WithImage("mongo:8.0.15")
            .WithDataVolume("stock-card-mongo-data")
            .WithMongoExpress(me => me.WithHostPort(5005)) // веб-интерфейс
            .AddDatabase("finantial-report-db");

        var notificationPostgres = builder.AddPostgres("notification-db")
            .WithImage("postgres:17.5")
            .WithHostPort(14054)
            .WithPgWeb(n => n.WithHostPort(5000))
            .AddDatabase("notificationDb");

        // Связывание ресурсов с проектами
        apiAuthService.WithReference(pgAuthDb)
                      .WaitFor(pgAuthDb);

        apiStockCardService
            .WithReference(redis)
            .WithReference(pgStockCardDb)
            .WithReference(mongoStockCardDb)
            .WaitFor(pgStockCardDb);

        apiPortfolioService
            .WithReference(redis)
            .WithReference(pgPortfolioDb)
            .WithReference(apiStockCardService)
            .WaitFor(pgPortfolioDb);

        apiAuthService
            .WithReference(pgAuthDb)
            .WithEnvironment("ConnectionStrings__Database", pgAuthDb.Resource.ConnectionStringExpression);

        apiNotificationService
            .WithReference(notificationPostgres)
            .WithReference(kafka)
            .WaitFor(notificationPostgres)
            .WaitFor(kafka);

        builder.Build().Run();
    }
}