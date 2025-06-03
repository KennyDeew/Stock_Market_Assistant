internal class Program
{
    private static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);
        var cache = builder.AddRedis("cache");

        //var container = builder.AddDockerfile(
        //    "gateway", "../backend/gateway/");
        builder.AddProject<Projects.Gateway_WebApi>("gateway-api");
        builder.AddProject<Projects.AuthService_WebApi>("authservice-api");
        builder.AddProject<Projects.AuthService_WebApi>("stockcardservice-api");
        builder.AddProject<Projects.AuthService_WebApi>("portfolioservice-api");
        builder.AddProject<Projects.AuthService_WebApi>("notificationservice-api");
        builder.Build().Run();
    }
}