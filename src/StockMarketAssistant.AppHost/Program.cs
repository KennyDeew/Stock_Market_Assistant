internal class Program
{
    private static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        // Добавление проектов

        var apiGatewayService = builder.AddProject<Projects.Gateway_WebApi>("gateway-api");
        var apiAuthService = builder.AddProject<Projects.AuthService_WebApi>("authservice-api");
        var authServiceMigrationJob = builder.AddProject<Projects.AuthService_MigrationsJob>("authservice-migrations-job");
        var apiStockCardService = builder.AddProject<Projects.StockCardService_WebApi>("stockcardservice-api");
        var apiPortfolioService = builder.AddProject<Projects.PortfolioService_WebApi>("portfolioservice-api");
        var apiAnalyticsService = builder.AddProject<Projects.AnalyticsService_WebApi>("analyticsservice-api");
        var apiNotificationService = builder.AddProject<Projects.NotificationService>("notificationservice-api");

        // Добавление ресурсов
        var redis = builder.AddRedis("cache");

        var kafka = builder.AddKafka("kafka")
            .WithKafkaUI(kafka => kafka.WithHostPort(9100));

        var openSearch = builder.AddContainer("opensearch", "opensearchproject/opensearch:2.11.0")
            .WithEnvironment("discovery.type", "single-node")
            .WithEnvironment("plugins.security.disabled", "true")
            .WithEnvironment("OPENSEARCH_INITIAL_ADMIN_PASSWORD", "admin")
            .WithHttpEndpoint(9200, 9200)
            .WithHttpEndpoint(9600, 9600, name: "performance-analyzer");

        var openSearchDashboards = builder.AddContainer("opensearch-dashboards", "opensearchproject/opensearch-dashboards:2.11.0")
            .WithEnvironment("OPENSEARCH_HOSTS", "http://opensearch:9200")
            .WithEnvironment("DISABLE_SECURITY_DASHBOARDS_PLUGIN", "true")
            .WithHttpEndpoint(5601, 5601)
            .WaitFor(openSearch);

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
            .WithDataVolume("stockcard-pg-data")
            .WithHostPort(14052)
            .WithPgWeb(n => n.WithHostPort(5001))
            .AddDatabase("stock-card-db");
        var mongoStockCardDb = builder.AddMongoDB("mongo-stock-card-db", 14053)
            //.WithLifetime(ContainerLifetime.Persistent)
            .WithImage("mongo:8.0.15")
            .WithDataVolume("stock-card-mongo-data")
            .WithMongoExpress(me => me.WithHostPort(5005)) // веб-интерфейс
            .AddDatabase("finantial-report-db");
        var pgNotificationDb = builder.AddPostgres("notification-db")
            .WithImage("postgres:17.5")
            .WithDataVolume("notification-pg-data")
            .WithHostPort(14054)
            .WithPgWeb(n => n.WithHostPort(5000))
            .AddDatabase("notificationDb");
        var pgAnalyticsDb = builder.AddPostgres("pg-analytics-db")
            //.WithPgAdmin()
            .WithImage("postgres:17.5")
            .WithDataVolume("analytics-pg-data")
            .WithHostPort(14055)
            .WithEnvironment("POSTGRES_USER", "postgres")
            // Пароль будет сгенерирован Aspire автоматически и будет одинаковым в контейнере и строке подключения
            .AddDatabase("analytics-db");
        //var postgres = builder.AddPostgres("postgres").AddDatabase("stockcarddb");
        //var container = builder.AddDockerfile("gateway", "../backend/gateway/");

        // Связывание ресурсов с проектами
        authServiceMigrationJob
            .WithReference(pgAuthDb)
            .WithEnvironment("ConnectionStrings__Database", pgAuthDb.Resource.ConnectionStringExpression)
            .WaitFor(pgAuthDb);

        apiAuthService
            .WithReference(pgAuthDb)
            .WithEnvironment("ConnectionStrings__Database", pgAuthDb.Resource.ConnectionStringExpression)
            .WaitFor(pgAuthDb)
            .WaitFor(authServiceMigrationJob);

        apiStockCardService
            .WithReference(redis)
            .WithReference(pgStockCardDb)
            .WithReference(mongoStockCardDb)
            .WaitFor(pgStockCardDb);

        apiPortfolioService
            .WithReference(redis)
            .WithReference(pgPortfolioDb)
            .WithReference(apiStockCardService)
            .WithReference(kafka)
            .WithEnvironment("OpenSearchConfig__Url", openSearch.GetEndpoint("http"))
            .WaitFor(pgPortfolioDb)
            .WaitFor(kafka);

        apiNotificationService
            .WithReference(pgNotificationDb)
            .WithReference(kafka)
            .WithEnvironment("OpenSearchConfig__Url", openSearch.GetEndpoint("http"))
            .WaitFor(pgNotificationDb)
            .WaitFor(kafka)
            .WaitFor(openSearch);

        // Добавление React-приложения
        //var webui = builder.AddExecutable(
        //    name: "webui",
        //    command: "npm",
        //    args: ["run", "dev"],
        //    workingDirectory: "../frontend")

        var webui = builder.AddNpmApp("webui", "../frontend", scriptName: "dev")
        //var webui = builder.AddContainer("webui", "webui")
        //.WithHttpEndpoint(port: 80, targetPort: 80)
        //.WithDockerfile("../frontend", "Dockerfile")
        .WithHttpEndpoint(
            port: 5273,           // порт, который будет открыт
            targetPort: 5273,     // порт, на котором слушает Vite
            name: "http",
            env: "PORT",
            isProxied: false
        )
        .WithEnvironment("VITE_AUTH_API_URL", apiAuthService.GetEndpoint("http"))
        .WithEnvironment("VITE_STOCKCARD_API_URL", apiStockCardService.GetEndpoint("http"))
        .WithEnvironment("VITE_PORTFOLIO_API_URL", apiPortfolioService.GetEndpoint("http"))
        .WithEnvironment("VITE_ANALYTICS_API_URL", apiAnalyticsService.GetEndpoint("http"))
        .WaitFor(apiAuthService)
        .WaitFor(apiStockCardService)
        .WaitFor(apiPortfolioService);
        // Используем явную строку подключения с паролем "xxx"
        // WithEnvironment после WithReference перезапишет ConnectionStringExpression от Aspire
        apiAnalyticsService
//            .WithEnvironment("ConnectionStrings__analytics-db",
  //              "Host=localhost;Port=14055;Database=analytics-db;Username=postgres;Password=xxx")
            .WithReference(pgAnalyticsDb)
            .WithReference(kafka)
            .WithReference(apiPortfolioService)
            .WithEnvironment("OpenSearchConfig__Url", openSearch.GetEndpoint("http"))
            .WithEnvironment("PortfolioService__BaseUrl", apiPortfolioService.GetEndpoint("http"))
            .WaitFor(pgAnalyticsDb)
            .WaitFor(kafka)
            .WaitFor(openSearch);


        var webuiUrl = webui.GetEndpoint("http"); // Получаем endpoint
        // Передаём URL фронтенда в бэкенды для CORS
        apiStockCardService.WithEnvironment("FRONTEND_ORIGIN", webuiUrl);
        apiAuthService.WithEnvironment("FRONTEND_ORIGIN", webuiUrl);
        apiPortfolioService.WithEnvironment("FRONTEND_ORIGIN", webuiUrl);
        apiAnalyticsService.WithEnvironment("FRONTEND_ORIGIN", webuiUrl);

        var app = builder.Build();

        // Открываем OpenSearch Dashboards в браузере после запуска (с задержкой для инициализации)
        //_ = Task.Run(async () =>
        //{
        //    await Task.Delay(TimeSpan.FromSeconds(10)); // Ждем 10 секунд для инициализации контейнера
        //    try
        //    {
        //        // Используем фиксированный URL, так как порт известен
        //        var openSearchDashboardsUrl = "http://localhost:5601";
        //        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        //        {
        //            FileName = openSearchDashboardsUrl,
        //            UseShellExecute = true
        //        });
        //    }
        //    catch
        //    {
        //        // Игнорируем ошибки открытия браузера
        //    }
        //});

        app.Run();
    }
}