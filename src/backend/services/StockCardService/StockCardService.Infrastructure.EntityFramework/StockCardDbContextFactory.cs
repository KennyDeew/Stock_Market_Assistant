using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using StockMarketAssistant.StockCardService.Data;
namespace StockMarketAssistant.StockCardService;

public class StockCardDbContextFactory : IDesignTimeDbContextFactory<StockCardDbContext>
{
	public StockCardDbContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<StockCardDbContext>();
		optionsBuilder.UseNpgsql("Your_Connection_String_Here");

		return new StockCardDbContext(optionsBuilder.Options);
	}
}