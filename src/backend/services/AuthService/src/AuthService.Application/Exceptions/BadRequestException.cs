using System.Text.Json;
using SharedKernel;

namespace AuthService.Application.Exceptions;

public class BadRequestException: Exception
{
    protected BadRequestException(Error[] errors)
        : base(JsonSerializer.Serialize(errors))
    {
    }
}