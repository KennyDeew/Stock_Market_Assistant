namespace StockCardService.WebApi.Models._01sub_Multiplier
{
    /// <summary>
    /// Модель мультипликатора акции
    /// </summary>
    public class MultiplierModel
    {
        /// <summary>
        /// Id мультипликатора
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Наименование мультипликатора
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Значение мультипликатора
        /// </summary>
        public decimal Value { get; set; }
    }
}
