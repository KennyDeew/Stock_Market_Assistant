using NJsonSchema;
using NJsonSchema.Generation;

namespace StockMarketAssistant.PortfolioService.WebApi.Infrastructure.Swagger
{
    internal class DefaultValueSchemaProcessor : ISchemaProcessor
    {
        public void Process(SchemaProcessorContext context)
        {
            var typeName = context.ContextualType?.Type?.Name;
            if (string.IsNullOrEmpty(typeName))
                return;

            if (
                    typeName.Equals("CreatePortfolioRequest", StringComparison.OrdinalIgnoreCase)
                        ||
                    typeName.Equals("CreatePortfolioAssetTransactionRequest", StringComparison.OrdinalIgnoreCase)
                        ||
                    typeName.Equals("UpdatePortfolioAssetTransactionRequest", StringComparison.OrdinalIgnoreCase)
                )
            {
                UpdateSchemaDefaults(context.Schema);
            }
        }

        private static void UpdateSchemaDefaults(JsonSchema schema)
        {
            if (schema?.Properties == null)
                return;

            // Currency
            if (schema.Properties.TryGetValue("currency", out var currencyProperty) && currencyProperty != null)
            {
                currencyProperty.Default = "RUB";
                currencyProperty.Example = "RUB";
            }

            // Quantity
            if (schema.Properties.TryGetValue("quantity", out var quantityProperty) && quantityProperty != null)
            {
                quantityProperty.Default = 1;
                quantityProperty.Example = 1;
            }
        }
    }
}
