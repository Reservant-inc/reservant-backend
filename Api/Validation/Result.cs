using FluentValidation.Results;

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

    private readonly TValue? _value;
    private readonly List<ValidationFailure>? _errors;

    /// <summary>
    /// Returned value or null if there are validation errors.
    /// </summary>
    public TValue Value =>
        !IsError
            ? _value!
            : throw new InvalidOperationException("Attempt to access the value of an erroneous Result");

    /// <summary>
    /// Validation errors or null if successful.
    /// </summary>
    public List<ValidationFailure> Errors =>
        IsError
            ? _errors!
            : throw new InvalidOperationException("Attempt to access the error list of a successful Result");

    /// <summary>
    /// Constructs a successful Result.
    /// </summary>
    public Result(TValue value)
    {
        IsError = false;
        _value = value;
        _errors = null;
    }

    /// <summary>
    /// Constructs a failed Result.
    /// </summary>
    public Result(List<ValidationFailure> errors)
    {
        IsError = true;
        _value = default;
        _errors = errors;
    }

    /// <summary>
    /// Allows to write <code>return value;</code> instead of <code>return new Result(value);</code>
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) => new(value);

    /// <summary>
    /// Convert a list of standard C# validation errors to Result
    /// </summary>
    public static implicit operator Result<TValue>(List<System.ComponentModel.DataAnnotations.ValidationResult> errors) =>
        new(ValidationUtils.ConvertToValidationFailures(errors).ToList());

    /// <summary>
    /// Allows to write <code>return result;</code> instead of <code>return new Result(result.Errors);</code>
    /// </summary>
    public static implicit operator Result<TValue>(ValidationResult result) => new(result.Errors);

    /// <summary>
    /// Allows to write <code>return errors;</code> instead of <code>return new Result(errors);</code>
    /// </summary>
    public static implicit operator Result<TValue>(List<ValidationFailure> errors) => new(errors);

    /// <summary>
    /// Create a Result with a single error
    /// </summary>
    /// <example>
    /// <code>
    /// return new ValidationFailure
    /// {
    ///     PropertyName = nameof(...),
    ///     ErrorCode = ...,
    ///     ErrorMessage = ...
    /// }
    /// </code>
    /// </example>
    public static implicit operator Result<TValue>(ValidationFailure error) => new([error]);

    /// <summary>
    /// Return the value, or throw an exception if there are validation errors.
    /// </summary>
    /// <exception cref="InvalidOperationException">If there are validation errors.</exception>
    public TValue OrThrow()
    {
        if (IsError)
        {
            throw new InvalidOperationException("Validation failed: " + string.Join(", ", Errors));
        }

        return Value!;
    }
}
