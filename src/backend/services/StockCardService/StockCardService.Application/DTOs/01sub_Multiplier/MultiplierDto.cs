namespace StockMarketAssistant.StockCardService.Application.DTOs._01sub_Multiplier
{
    /// <summary>
    /// Dto мультипликатора акции
    /// </summary>
    public class MultiplierDto
    {
        /// <summary>
        /// Id мультипликатора
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Наименование мультипликатора
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Значение мультипликатора
        /// </summary>
        public decimal Value { get; set; }
    }
}
