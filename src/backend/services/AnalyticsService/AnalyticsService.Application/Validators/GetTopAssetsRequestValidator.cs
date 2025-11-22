using FluentValidation;
using StockMarketAssistant.AnalyticsService.Application.DTOs.Requests;
using StockMarketAssistant.AnalyticsService.Domain.Constants;
using StockMarketAssistant.AnalyticsService.Domain.Enums;

namespace StockMarketAssistant.AnalyticsService.Application.Validators
{
    /// <summary>
    /// Валидатор для GetTopAssetsRequest
    /// </summary>
    public class GetTopAssetsRequestValidator : AbstractValidator<GetTopAssetsRequest>
    {
        public GetTopAssetsRequestValidator()
        {
            RuleFor(x => x.StartDate)
                .NotEmpty()
                .WithMessage("Начальная дата обязательна");

            RuleFor(x => x.EndDate)
                .NotEmpty()
                .WithMessage("Конечная дата обязательна")
                .GreaterThan(x => x.StartDate)
                .WithMessage("Конечная дата должна быть больше начальной даты");

            RuleFor(x => x.Top)
                .GreaterThan(0)
                .WithMessage("Количество топ активов должно быть больше 0")
                .LessThanOrEqualTo(DomainConstants.Aggregation.MaxTopAssetsCount)
                .WithMessage($"Количество топ активов не должно превышать {DomainConstants.Aggregation.MaxTopAssetsCount}");

            RuleFor(x => x.Context)
                .IsInEnum()
                .WithMessage("Недопустимый контекст анализа");

            RuleFor(x => x.PortfolioId)
                .NotEmpty()
                .When(x => x.Context == AnalysisContext.Portfolio)
                .WithMessage("Идентификатор портфеля обязателен для контекста Portfolio");

            RuleFor(x => x.PortfolioId)
                .Empty()
                .When(x => x.Context == AnalysisContext.Global)
                .WithMessage("Идентификатор портфеля должен быть пустым для контекста Global");
        }
    }
}

