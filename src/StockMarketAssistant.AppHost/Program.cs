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
        var pgAnalyticsDb = builder.AddPostgres("pg-analytics-db")
            //.WithPgAdmin()
            .WithImage("postgres:17.5")
            .WithDataVolume("analytics-pg-data")
            .WithHostPort(14051)
            .AddDatabase("analytics-db")
            .WithCredentials("postgres", "xxxxxx");
        //var postgres = builder.AddPostgres("postgres").AddDatabase("stockcarddb");
        //var container = builder.AddDockerfile("gateway", "../backend/gateway/");

        // Связывание ресурсов с проектами
        apiAuthService
            .WithReference(pgAuthDb)
            .WithEnvironment("ConnectionStrings__Database", pgAuthDb.Resource.ConnectionStringExpression)
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

        apiNotificationService
            .WithReference(notificationPostgres)
            .WithReference(kafka)
            .WaitFor(notificationPostgres)
            .WaitFor(kafka);

        // Добавление React-приложения
        //var webui = builder.AddExecutable(
        //    name: "webui",
        //    command: "npm",
        //    args: ["run", "dev"],
        //    workingDirectory: "../frontend")

        var webui = builder.AddNpmApp("webui", "../frontend", scriptName: "dev")
        .WithHttpEndpoint(
            port: 5173,           // порт, который будет открыт
            targetPort: 5173,     // порт, на котором слушает Vite
            name: "http",
            env: "PORT",
            isProxied: false
        )
        .WithEnvironment("VITE_AUTH_API_URL", apiAuthService.GetEndpoint("http"))
        .WithEnvironment("VITE_PORTFOLIO_API_URL", apiPortfolioService.GetEndpoint("http"))
        .WithEnvironment("VITE_STOCKCARD_API_URL", apiStockCardService.GetEndpoint("http"))
        .WithEnvironment("VITE_NOTIFICATION_API_URL", apiNotificationService.GetEndpoint("http"))
        .WaitFor(apiAuthService)
        .WaitFor(apiStockCardService);
        apiAnalyticsService.WithReference(pgAnalyticsDb)
                           .WaitFor(pgAnalyticsDb);


        var webuiUrl = webui.GetEndpoint("http"); // Получаем endpoint
        // Передаём URL фронтенда в бэкенды для CORS
        apiStockCardService.WithEnvironment("FRONTEND_ORIGIN", webuiUrl);
        apiAuthService.WithEnvironment("FRONTEND_ORIGIN", webuiUrl);
        apiPortfolioService.WithEnvironment("FRONTEND_ORIGIN", webuiUrl);
        builder.Build().Run();
    }
}