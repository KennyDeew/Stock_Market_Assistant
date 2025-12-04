namespace SharedKernel;

public static class Errors
{
    public static class General
    {
        public static Error ValueIsInvalid(string? name = null)
        {
            string label = name ?? "Значение";
            return Error.Validation(
                "core.shared.value.validate.invalid",
                $"{label} недопустимо",
                name);
        }

        public static Error NotFound(string? id = null, string? name = null)
        {
            string label = name ?? "Запись";
            string forId = id == null ? string.Empty : $" (Id: {id})";
            return Error.NotFound(
                "core.shared.record.get.not-found",
                $"{label} не найдена{forId}");
        }

        public static Error ValueIsRequired(string? name = null)
        {
            string label = name ?? "Значение";
            return Error.Validation(
                "core.shared.value.validate.required",
                $"{label} является обязательным",
                name);
        }

        public static Error AlreadyExist(string? name = null)
        {
            string label = name ?? "Запись";
            return Error.Conflict(
                "core.shared.record.create.already-exists",
                $"{label} уже существует");
        }

        public static Error Failure()
        {
            return Error.Failure(
                "core.shared.operation.execute.failure",
                "Произошла ошибка");
        }

        public static Error NotAllowed(string? action = null)
        {
            string suffix = action is null ? string.Empty : $": {action}";
            return Error.Failure(
                "core.shared.operation.perform.not-allowed",
                $"Действие запрещено{suffix}");
        }
    }

    public static class Tokens
    {
        public static Error ExpiredToken()
        {
            return Error.Validation(
                "core.auth.token.refresh.expired",
                "Срок действия токена истёк");
        }

        public static Error InvalidToken()
        {
            return Error.Validation(
                "core.auth.token.validate.invalid",
                "Токен недействителен");
        }
    }

    public static class User
    {
        public static Error InvalidCredentials()
        {
            return Error.Validation(
                "core.auth.user.validate.credentials-invalid",
                "Неверные учётные данные");
        }
    }
}