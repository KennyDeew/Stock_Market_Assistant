using System.ComponentModel;

namespace StockMarketAssistant.PortfolioService.Application.Services
{
    public static class Extensions
    {
        public static string GetEnumDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field is null)
                return string.Empty;
            var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute?.Description ?? value.ToString(); // Возвращает описание или имя enum
        }
    }
}
