namespace SharedKernel;

public record Envelope
{
    public object? Result { get; }
    public ErrorList? Errors { get; }
    public DateTime TimeGenerated { get; }

    private Envelope(object? result, ErrorList? errors)
    {
        Result = result;
        Errors = errors;
        TimeGenerated = DateTime.UtcNow;
    }

    public static Envelope Ok(object? result = null)
    {
        return new Envelope(result, null);
    }

    public static Envelope Error(ErrorList errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        return new Envelope(null, errors);
    }

    public static Envelope Error(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Envelope(null, error.ToErrorList());
    }
}