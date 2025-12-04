using Yarp.ReverseProxy.Configuration;

namespace StockMarketAssistant.Gateway.WebApi
{
    /// <summary>
    /// Точка входа приложения Gateway с использованием YARP для reverse proxy
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.AddServiceDefaults();

            // Добавляем YARP Reverse Proxy
            builder.Services.AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

            // CORS для фронтенда
            var frontendOrigin = builder.Configuration["FRONTEND_ORIGIN"] ?? "http://localhost:5273";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontendApp", policy =>
                {
                    policy
                        .WithOrigins(frontendOrigin)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            // OpenAPI для документации (опционально)
            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Stock Market Assistant API Gateway",
                    Version = "v1",
                    Description = "API Gateway для микросервисов Stock Market Assistant"
                });
            });

            var app = builder.Build();

            app.MapDefaultEndpoints();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway V1");
                    c.RoutePrefix = "swagger";
                });
                app.MapOpenApi();
            }

            app.UseCors("AllowFrontendApp");
            app.UseHttpsRedirection();
            app.UseAuthorization();

            // Маппинг YARP Reverse Proxy - должен быть последним в pipeline
            app.MapReverseProxy();

            app.Run();
        }
    }
}
