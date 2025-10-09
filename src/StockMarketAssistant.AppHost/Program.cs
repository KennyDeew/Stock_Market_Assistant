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
        //var postgres = builder.AddPostgres("postgres").AddDatabase("stockcarddb");
        //var container = builder.AddDockerfile("gateway", "../backend/gateway/");

        // Связывание ресурсов с проектами
        apiPortfolioService.WithReference(redis);
        //apiPortfolioService.WithReference(postgres);


        builder.Build().Run();
    }
}