namespace SweetFlowerShop.Application.Common;

public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public IReadOnlyList<string> Errors { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Errors = [];
    }

    private Result(string error)
    {
        IsSuccess = false;
        Value = default;
        Error = error;
        Errors = [error];
    }

    private Result(IEnumerable<string> errors)
    {
        var list = errors.ToList();
        IsSuccess = false;
        Value = default;
        Error = list.FirstOrDefault();
        Errors = list;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error) => new(error);
    public static Result<T> Failure(IEnumerable<string> errors) => new(errors);
}
