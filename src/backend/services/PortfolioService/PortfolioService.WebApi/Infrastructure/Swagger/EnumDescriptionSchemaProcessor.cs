using NJsonSchema.Generation;
using System.ComponentModel;
using System.Reflection;

namespace StockMarketAssistant.PortfolioService.WebApi.Infrastructure.Swagger
{
    internal class EnumDescriptionSchemaProcessor : ISchemaProcessor
    {
        public void Process(SchemaProcessorContext context)
        {
            var contextType = context.ContextualType.Type;
            if (contextType.IsEnum)
            {
                var schema = context.Schema;

                var descriptions = new List<string>();

                foreach (string name in Enum.GetNames(contextType))
                {
                    var field = contextType.GetField(name);
                    var value = Convert.ToInt32(Enum.Parse(contextType, name));

                    var descriptionAttr = field?.GetCustomAttribute<DescriptionAttribute>();
                    var displayAttr = field?.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();

                    var description = descriptionAttr?.Description
                                   ?? displayAttr?.Name
                                   ?? name;

                    descriptions.Add($"{value} - {description}");
                }

                if (descriptions.Count > 0)
                {
                    schema.Description = string.Join(", ", descriptions);
                }
            }
        }
    }
}
