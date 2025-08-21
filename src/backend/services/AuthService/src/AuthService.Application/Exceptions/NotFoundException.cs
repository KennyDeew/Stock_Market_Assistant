using System.Text.Json;
using SharedKernel;

namespace AuthService.Application.Exceptions;

public class NotFoundException: Exception
{
    protected NotFoundException(Error[] errors)
        : base(JsonSerializer.Serialize(errors))
    {
    }
}