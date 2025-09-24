using System;
using System.Collections;
using System.Collections.Generic;

namespace SharedKernel;

public class ErrorList : IEnumerable<Error>
{
    private readonly List<Error> _errors;

    public ErrorList(IEnumerable<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        _errors = new List<Error>(errors);
    }

    public static ErrorList From(Error error) =>
        new ErrorList(new[] { error });

    public static ErrorList From(IEnumerable<Error> errors) =>
        new ErrorList(errors);

    public static readonly ErrorList Empty = new(Array.Empty<Error>());

    public int Count => _errors.Count;

    public bool IsEmpty => _errors.Count == 0;

    public IEnumerator<Error> GetEnumerator() => _errors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static implicit operator ErrorList(List<Error> errors) => new(errors);

    public static implicit operator ErrorList(Error error) => new(new[] { error });
}