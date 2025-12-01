using FluentValidation;
using StockMarketAssistant.PortfolioService.WebApi.Models;

namespace StockMarketAssistant.PortfolioService.WebApi.Validators;

/// <summary>
/// Валидатор для модели редактирования транзакции актива портфеля <see cref="UpdatePortfolioAssetTransactionRequest"/>.
/// Обеспечивает проверку корректности входных данных при обновлении существующей транзакции.
/// </summary>
public class UpdatePortfolioAssetTransactionRequestValidator : AbstractValidator<UpdatePortfolioAssetTransactionRequest>
{
    /// <summary>
    /// Инициализирует правила валидации для модели <see cref="UpdatePortfolioAssetTransactionRequest"/>.
    /// </summary>
    public UpdatePortfolioAssetTransactionRequestValidator()
    {
        // Проверка типа транзакции (enum)
        RuleFor(x => x.TransactionType)
            .IsInEnum()
            .WithMessage("Тип операции должен быть допустимым значением (Buy или Sell).");

        // Проверка цены за единицу
        RuleFor(x => x.PricePerUnit)
            .GreaterThan(0)
            .WithMessage("Цена за единицу должна быть больше нуля.");

        // Проверка количества
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Количество должно быть больше нуля.");

        // Проверка даты транзакции
        RuleFor(x => x.TransactionDate)
            .NotEmpty()
            .WithMessage("Дата операции обязательна.")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Дата операции не может быть в будущем.");

        // Проверка валюты
        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Валюта операции обязательна.")
            .MinimumLength(3)
            .WithMessage("Валюта должна содержать минимум 3 символа.")
            .MaximumLength(10)
            .WithMessage("Валюта не должна превышать 10 символов.")
            .Matches("^[A-Z]+$")
            .WithMessage("Валюта должна состоять только из заглавных латинских букв (например: RUB, USD).");
    }
}