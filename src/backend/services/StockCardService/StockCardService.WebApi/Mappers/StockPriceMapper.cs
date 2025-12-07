using StockCardService.WebApi.Models.ShareCard;
using StockMarketAssistant.StockCardService.Application.DTOs;
using StockMarketAssistant.StockCardService.WebApi.Models;

namespace StockMarketAssistant.StockCardService.WebApi.Mappers
{
    /// <summary>
    /// Маппер для StockPrice model/dto.
    /// </summary>
    public static class StockPriceMapper
    {
        /// <summary>
        /// Маппинг StockPrice Dto=>Model
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static StockPriceModel ToModel(StockPriceDto dto)
        {
            return new StockPriceModel
            {
                Ticker = dto.Ticker,
                Price = dto.Price,
                Timestamp = dto.Timestamp
            };
        }
    }
}
