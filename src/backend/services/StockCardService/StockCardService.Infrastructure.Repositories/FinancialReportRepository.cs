using StockCardService.Abstractions.Repositories;
using StockMarketAssistant.StockCardService.Domain.Entities;

namespace StockMarketAssistant.StockCardService.Infrastructure.Repositories
{
    public class FinancialReportRepository : MongoDbRepository<FinancialReport, Guid>
    {
        public FinancialReportRepository(IMongoDBContext context) : base(context)
        {

        }
    }
}
