using FluentValidation;
using StockMarketAssistant.PortfolioService.WebApi.Models;

namespace StockMarketAssistant.PortfolioService.WebApi.Validators;

/// <summary>
/// Валидатор для модели создания портфеля <see cref="CreatePortfolioRequest"/>.
/// Обеспечивает проверку корректности входных данных при создании нового портфеля.
/// </summary>
public class CreatePortfolioRequestValidator : AbstractValidator<CreatePortfolioRequest>
{
    /// <summary>
    /// Инициализирует правила валидации для модели <see cref="CreatePortfolioRequest"/>.
    /// </summary>
    public CreatePortfolioRequestValidator()
    {
        // Проверка идентификатора пользователя
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty)
            .WithMessage("Идентификатор пользователя не может быть пустым.");

        // Проверка наименования портфеля
        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage("Наименование портфеля обязательно для заполнения.")
            .MaximumLength(100)
            .WithMessage("Наименование портфеля не должно превышать 100 символов.");

        // Проверка валюты портфеля
        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Валюта портфеля обязательна для указания.")
            .Matches("^[A-Z]{3}$")
            .WithMessage("Валюта должна быть указана в формате ISO 4217 (3 заглавные латинские буквы, например: RUB, USD, EUR).");
    }
}