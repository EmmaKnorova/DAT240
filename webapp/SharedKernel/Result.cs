using Microsoft.AspNetCore.Identity;

namespace TarlBreuJacoBaraKnor.SharedKernel;

public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public List<string> Errors { get; init; } = [];

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(IEnumerable<string> errors) => new() { IsSuccess = false, Errors = errors.ToList() };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, Errors = [error] };
    public static Result<T> Failure(IdentityResult identityResult) =>
        Failure(identityResult.Errors.Select(e => e.Description));
}

public record Result : Result<object>
{
    public static Result Success() => new() { IsSuccess = true, Value = null };
    public new static Result Failure(IEnumerable<string> errors) => new() { IsSuccess = false, Errors = errors.ToList() };
    public new static Result Failure(string error) => new() { IsSuccess = false, Errors = [error] };
    public new static Result Failure(IdentityResult identityResult) =>
        Failure(identityResult.Errors.Select(e => e.Description));
}