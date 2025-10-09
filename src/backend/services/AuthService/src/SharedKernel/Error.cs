using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SharedKernel;

public record Error
{
    private static readonly Regex _codeRegex = new Regex(
        @"^[a-z][a-z0-9-]*(\.[a-z][a-z0-9-]*){2,}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() },
    };

    private sealed record ErrorDto(
        int v,
        string code,
        string message,
        ErrorType type,
        string? invalidField
    );

    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }
    public string? InvalidField { get; }

    private Error(string code, string message, ErrorType type, string? invalidField = null)
    {
        ValidateCodeOrThrow(code);
        Code = code;
        Message = message;
        Type = type;
        InvalidField = invalidField;
    }

    public static Error Validation(string code, string message, string? invalidField = null)
    {
        return new Error(code, message, ErrorType.VALIDATION, invalidField);
    }

    public static Error NotFound(string code, string message)
    {
        return new Error(code, message, ErrorType.NOT_FOUND);
    }

    public static Error Failure(string code, string message)
    {
        return new Error(code, message, ErrorType.FAILURE);
    }

    public static Error Conflict(string code, string message)
    {
        return new Error(code, message, ErrorType.CONFLICT);
    }

    public string Serialize()
    {
        ErrorDto dto = new ErrorDto(v: 1, code: Code, message: Message, type: Type, invalidField: InvalidField);
        return JsonSerializer.Serialize(dto, _jsonOpts);
    }

    public static Error Deserialize(string serialized)
    {
        ErrorDto dto;
        try
        {
            dto = JsonSerializer.Deserialize<ErrorDto>(serialized, _jsonOpts)
                  ?? throw new ArgumentException("Некорректный формат сериализованной ошибки.");
        }
        catch (JsonException)
        {
            throw new ArgumentException("Ожидался JSON-формат ошибки.");
        }

        if (dto.v < 1)
        {
            throw new ArgumentException("Неподдерживаемая версия формата сериализации ошибки.");
        }

        return new Error(dto.code, dto.message, dto.type, dto.invalidField);
    }

    public ErrorList ToErrorList()
    {
        return new ErrorList(new Error[] { this });
    }

    private static void ValidateCodeOrThrow(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || !_codeRegex.IsMatch(code))
        {
            throw new ArgumentException(
                $"Недопустимый формат кода ошибки: '{code}'.");
        }
    }
}