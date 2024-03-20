using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Validation;

/// <summary>
/// Used to return a value in case of success OR validation errors if any.
/// </summary>
/// <typeparam name="TValue">The value type in case of success.</typeparam>
public readonly struct Result<TValue>
{
    /// <summary>
    /// Whether there are validation errors or not.
    /// </summary>
    public bool IsError { get; }

    /// <summary>
    /// Returned value or null if there are validation errors.
    /// </summary>
    public TValue? Value { get; }

    /// <summary>
    /// Validation errors or null if successful.
    /// </summary>
    public List<ValidationResult>? Errors { get; }

    /// <summary>
    /// Constructs a successful Result.
    /// </summary>
    public Result(TValue value)
    {
        IsError = false;
        Value = value;
        Errors = null;
    }

    /// <summary>
    /// Constructs a failed Result.
    /// </summary>
    public Result(List<ValidationResult> errors)
    {
        IsError = true;
        Value = default;
        Errors = errors;
    }

    /// <summary>
    /// Allows to write <code>return value;</code> instead of <code>return new Result(value);</code>
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) => new(value);

    /// <summary>
    /// Allows to write <code>return errors;</code> instead of <code>return new Result(errors);</code>
    /// </summary>
    public static implicit operator Result<TValue>(List<ValidationResult> errors) => new(errors);

    /// <summary>
    /// Return the value, or throw an exception if there are validation errors.
    /// </summary>
    /// <exception cref="InvalidOperationException">If there are validation errors.</exception>
    public TValue OrThrow()
    {
        if (IsError)
        {
            throw new InvalidOperationException("Validation failed: " + string.Join(", ", Errors!));
        }

        return Value!;
    }
}
