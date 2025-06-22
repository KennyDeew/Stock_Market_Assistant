using StockCardService.Abstractions.Repositories;
using StockMarketAssistant.StockCardService.Domain.Entities;
using StockCardService.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Data;
using StockMarketAssistant.StockCardService.Infrastructure.EntityFramework;

namespace StockCardService.WebApi
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddScoped(typeof(IRepository<ShareCard, Guid>), typeof(ShareCardRepository));
            services.AddScoped(typeof(IRepository<BondCard, Guid>), typeof(BondCardRepository));
            services.AddScoped(typeof(IRepository<CryptoCard, Guid>), typeof(CryptoCardRepository));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, StockCardDbContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            //context.Database.Migrate();

            app.UseOpenApi();
            app.UseSwaggerUi(x =>
            {
                x.DocExpansion = "list";
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
