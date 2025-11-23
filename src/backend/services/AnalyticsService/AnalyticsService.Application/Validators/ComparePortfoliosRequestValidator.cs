using FluentValidation;
using StockMarketAssistant.AnalyticsService.Application.DTOs.Requests;
using StockMarketAssistant.AnalyticsService.Domain.Constants;

namespace StockMarketAssistant.AnalyticsService.Application.Validators
{
    /// <summary>
    /// Валидатор для ComparePortfoliosRequest
    /// </summary>
    public class ComparePortfoliosRequestValidator : AbstractValidator<ComparePortfoliosRequest>
    {
        public ComparePortfoliosRequestValidator()
        {
            RuleFor(x => x.PortfolioIds)
                .NotEmpty()
                .WithMessage("Необходимо указать хотя бы один портфель для сравнения")
                .Must(ids => ids.Count <= DomainConstants.Validation.MaxPortfoliosInComparison)
                .WithMessage($"Максимальное количество портфелей для сравнения: {DomainConstants.Validation.MaxPortfoliosInComparison}")
                .Must(ids => ids.All(id => id != Guid.Empty))
                .WithMessage("Идентификаторы портфелей не могут быть пустыми")
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage("Идентификаторы портфелей должны быть уникальными");

            RuleFor(x => x.StartDate)
                .LessThan(x => x.EndDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("Начальная дата должна быть меньше конечной даты");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("Конечная дата должна быть больше начальной даты");
        }
    }
}

