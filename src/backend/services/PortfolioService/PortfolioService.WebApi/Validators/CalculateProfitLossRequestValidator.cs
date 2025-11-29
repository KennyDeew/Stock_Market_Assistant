using FluentValidation;
using StockMarketAssistant.PortfolioService.WebApi.Models;

namespace StockMarketAssistant.PortfolioService.WebApi.Validators;

/// <summary>
/// Валидатор для модели запроса расчёта доходности <see cref="CalculateProfitLossRequest"/>.
/// Обеспечивает проверку корректности типа расчёта доходности.
/// </summary>
public class CalculateProfitLossRequestValidator : AbstractValidator<CalculateProfitLossRequest>
{
    /// <summary>
    /// Инициализирует правила валидации для модели <see cref="CalculateProfitLossRequest"/>.
    /// </summary>
    public CalculateProfitLossRequestValidator()
    {
        // Проверка типа расчёта доходности (enum)
        RuleFor(x => x.CalculationType)
            .IsInEnum()
            .WithMessage("Тип расчёта доходности должен быть допустимым значением (Current или Realized).");
    }
}