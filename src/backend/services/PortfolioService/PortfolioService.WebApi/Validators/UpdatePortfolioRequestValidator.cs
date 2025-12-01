using FluentValidation;
using StockMarketAssistant.PortfolioService.WebApi.Models;

namespace StockMarketAssistant.PortfolioService.WebApi.Validators;

/// <summary>
/// Валидатор для модели обновления портфеля <see cref="UpdatePortfolioRequest"/>.
/// Проверяет корректность данных при редактировании существующего портфеля.
/// </summary>
public class UpdatePortfolioRequestValidator : AbstractValidator<UpdatePortfolioRequest>
{
    /// <summary>
    /// Инициализирует правила валидации для модели <see cref="UpdatePortfolioRequest"/>.
    /// </summary>
    public UpdatePortfolioRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Наименование портфеля обязательно.")
            .MaximumLength(100)
            .WithMessage("Наименование портфеля не должно превышать 100 символов.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Валюта портфеля обязательна.")
            .Matches("^[A-Z]{3}$")
            .WithMessage("Валюта должна быть в формате ISO 4217 (3 заглавные латинские буквы).");
    }
}