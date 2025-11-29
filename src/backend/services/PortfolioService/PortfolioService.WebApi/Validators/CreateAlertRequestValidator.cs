using FluentValidation;
using StockMarketAssistant.PortfolioService.WebApi.Models;

namespace StockMarketAssistant.PortfolioService.WebApi.Validators;

/// <summary>
/// Валидатор для модели создания оповещения <see cref="CreateAlertRequest"/>.
/// Обеспечивает проверку корректности входных данных при создании нового оповещения о цене актива.
/// </summary>
public class CreateAlertRequestValidator : AbstractValidator<CreateAlertRequest>
{
    /// <summary>
    /// Инициализирует правила валидации для модели <see cref="CreateAlertRequest"/>.
    /// </summary>
    public CreateAlertRequestValidator()
    {
        // Обязательные поля
        RuleFor(x => x.StockCardId)
            .NotEqual(Guid.Empty)
            .WithMessage("Идентификатор карточки актива обязателен.");

        RuleFor(x => x.AssetType)
            .IsInEnum()
            .WithMessage("Тип актива должен быть допустимым значением (Stock, Bond, Crypto).");

        RuleFor(x => x.TargetPrice)
            .GreaterThan(0)
            .WithMessage("Целевая цена должна быть положительной.")
            .LessThanOrEqualTo(1_000_000_000)
            .WithMessage("Целевая цена не должна превышать 1 000 000 000.");

        RuleFor(x => x.Condition)
            .IsInEnum()
            .WithMessage("Условие срабатывания должно быть 'PriceAbove' или 'PriceBelow'.");

        // Опциональные, но с ограничениями при наличии
        RuleFor(x => x.AssetTicker)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.AssetTicker))
            .WithMessage("Тикер актива не должен превышать 20 символов.");

        RuleFor(x => x.AssetName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.AssetName))
            .WithMessage("Название актива не должно превышать 100 символов.");

        RuleFor(x => x.AssetCurrency)
            .Matches("^[A-Z]{3}$")
            .When(x => !string.IsNullOrEmpty(x.AssetCurrency))
            .WithMessage("Валюта актива должна быть в формате ISO 4217 (3 заглавные латинские буквы, например: RUB, USD).");
    }
}