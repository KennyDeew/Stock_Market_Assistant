using StockMarketAssistant.StockCardService.Application.DTOs._01_ShareCard;
using StockMarketAssistant.StockCardService.Application.DTOs._01sub_Dividend;
using StockMarketAssistant.StockCardService.Application.DTOs._01sub_Multiplier;
using StockMarketAssistant.StockCardService.Domain.Entities;

namespace StockMarketAssistant.StockCardService.Tests.Builders
{
    public class ShareCardBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _ticker = "SBER";
        private string _name = "Сбербанк России ПАО - обыкн.";
        private string _description = "ПАО «СберБанк России» занимается оказанием банковских и финансовых услуг";
        private string _currency = "RUB";
        private decimal _currentPrice = 0m;
        private List<MultiplierDto> _multipliers = new();
        private List<DividendDto> _dividends = new();

        public ShareCardBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public ShareCardBuilder WithTicker(string ticker)
        {
            _ticker = ticker;
            return this;
        }

        public ShareCardBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ShareCardBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public ShareCardBuilder WithCurrency(string currency)
        {
            _currency = currency;
            return this;
        }

        public ShareCardBuilder WithCurrentPrice(decimal price)
        {
            _currentPrice = price;
            return this;
        }

        public ShareCardBuilder WithMultipliers(List<MultiplierDto> multipliers)
        {
            _multipliers = multipliers;
            return this;
        }

        public ShareCardBuilder WithDividends(List<DividendDto> dividends)
        {
            _dividends = dividends;
            return this;
        }

        public ShareCard Build()
        {
            return new ShareCard
            {
                Id = _id,
                Ticker = _ticker,
                Name = _name,
                Description = _description,
                Currency = _currency,
                CurrentPrice = _currentPrice,
                Multipliers = (ICollection<Multiplier>)_multipliers,
                Dividends = (ICollection<Dividend>)_dividends
            };
        }

        /// <summary>
        /// Преобразует ShareCardBuilder в ShareCardDto
        /// </summary>
        public ShareCardDto ToDto()
        {
            return new ShareCardDto
            {
                Id = _id,
                Ticker = _ticker,
                Name = _name,
                Description = _description,
                Currency = _currency,
                CurrentPrice = _currentPrice,
                Multipliers = _multipliers,
                Dividends = _dividends
            };
        }

        /// <summary>
        /// Преобразует ShareCardBuilder в ShareCardShortDto
        /// </summary>
        public ShareCardShortDto ToShortDto()
        {
            return new ShareCardShortDto
            {
                Id = _id,
                Ticker = _ticker,
                Name = _name,
                Description = _description,
                Currency = _currency,
                CurrentPrice = _currentPrice
            };
        }
    }
}
