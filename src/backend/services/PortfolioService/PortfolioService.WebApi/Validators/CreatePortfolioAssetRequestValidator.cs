using FluentValidation;
using StockMarketAssistant.PortfolioService.WebApi.Models;

namespace StockMarketAssistant.PortfolioService.WebApi.Validators;

/// <summary>
/// Валидатор для модели создания актива портфеля <see cref="CreatePortfolioAssetRequest"/>.
/// Обеспечивает проверку корректности входных данных при добавлении нового актива в портфель.
/// </summary>
public class CreatePortfolioAssetRequestValidator : AbstractValidator<CreatePortfolioAssetRequest>
{
    /// <summary>
    /// Инициализирует правила валидации для модели <see cref="CreatePortfolioAssetRequest"/>.
    /// </summary>
    public CreatePortfolioAssetRequestValidator()
    {
        // Проверка идентификатора портфеля
        RuleFor(x => x.PortfolioId)
            .NotEqual(Guid.Empty)
            .WithMessage("Идентификатор портфеля обязателен.");

        // Проверка идентификатора карточки ценной бумаги
        RuleFor(x => x.StockCardId)
            .NotEqual(Guid.Empty)
            .WithMessage("Идентификатор карточки ценной бумаги обязателен.");

        // Проверка типа актива (enum)
        RuleFor(x => x.AssetType)
            .IsInEnum()
            .WithMessage("Тип актива должен быть допустимым значением (Share, Bond, Crypto).");

        // Проверка цены покупки за единицу
        RuleFor(x => x.PurchasePricePerUnit)
            .GreaterThan(0)
            .WithMessage("Цена покупки за единицу должна быть больше нуля.");

        // Проверка количества
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Количество должно быть больше нуля.");
    }
}