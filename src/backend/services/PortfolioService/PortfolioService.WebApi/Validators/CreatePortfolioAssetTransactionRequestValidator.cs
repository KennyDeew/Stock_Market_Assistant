using FluentValidation;
using StockMarketAssistant.PortfolioService.WebApi.Models;

namespace StockMarketAssistant.PortfolioService.WebApi.Validators;

/// <summary>
/// Валидатор для модели создания транзакции актива портфеля <see cref="CreatePortfolioAssetTransactionRequest"/>.
/// Обеспечивает проверку корректности входных данных при добавлении новой транзакции к активу портфеля.
/// </summary>
public class CreatePortfolioAssetTransactionRequestValidator : AbstractValidator<CreatePortfolioAssetTransactionRequest>
{
    /// <summary>
    /// Инициализирует правила валидации для модели <see cref="CreatePortfolioAssetTransactionRequest"/>.
    /// </summary>
    public CreatePortfolioAssetTransactionRequestValidator()
    {
        // Проверка типа транзакции (enum)
        RuleFor(x => x.TransactionType)
            .IsInEnum()
            .WithMessage("Тип операции должен быть допустимым значением (Buy или Sell).");

        // Проверка количества
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Количество должно быть больше нуля.");

        // Проверка цены за единицу
        RuleFor(x => x.PricePerUnit)
            .GreaterThan(0)
            .WithMessage("Цена за единицу должна быть больше нуля.");

        // Проверка даты транзакции
        RuleFor(x => x.TransactionDate)
            .NotEmpty()
            .WithMessage("Дата операции обязательна.")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Дата операции не может быть в будущем.");
    }
}