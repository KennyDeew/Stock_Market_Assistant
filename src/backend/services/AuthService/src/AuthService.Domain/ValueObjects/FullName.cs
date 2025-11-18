using CSharpFunctionalExtensions;
using SharedKernel;

namespace AuthService.Domain.ValueObjects;

public sealed class FullName
{
    private FullName(string firstName, string secondName)
    {
        FirstName = firstName;
        SecondName = secondName;
    }

    public string FirstName { get; }

    public string SecondName { get; }

    public static Result<FullName, Error> Create(string firstName, string secondName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Errors.General.ValueIsInvalid("FirstName");

        if (string.IsNullOrWhiteSpace(secondName))
            return Errors.General.ValueIsInvalid("SecondName");

        return new FullName(firstName, secondName);
    }
}