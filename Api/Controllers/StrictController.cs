using Microsoft.AspNetCore.Mvc;

namespace Reservant.Api.Controllers;

/// <summary>
/// Base controller class with custom convenience method overloads
/// </summary>
public class StrictController : Controller
{
    /// <summary>
    /// Represents an Ok result without a value. Can only be converted to an
    /// <see cref="ActionResult"/>, and not to an <see cref="ActionResult{T}"/>
    /// to enforce the correct declared return type.
    /// </summary>
    public class StrictEmptyActionResult(ActionResult inner)
    {
        private readonly ActionResult _inner = inner;

        /// <summary>
        /// Implicit conversion to an <see cref="ActionResult"/>
        /// </summary>
        public static implicit operator ActionResult(StrictEmptyActionResult strictResult) => strictResult._inner;
    }

    /// <summary>
    /// Wrapper around the base Ok method, but returns an <see cref="ActionResult{T}"/>,
    /// to ensure the declared return type matches the actual returned value's type
    /// </summary>
    protected ActionResult<T> Ok<T>(T value) => base.Ok(value);

    /// <summary>
    /// Wrapper around the base Ok method, but returns a wrapped result
    /// to ensure the declared return type matches the actual returned value's type
    /// </summary>
    protected new StrictEmptyActionResult Ok() => new(base.Ok());

    /// <summary>
    /// Wrapper around the base NoContent method, but returns a wrapped
    /// to ensure the declared return type matches the actual returned value's type
    /// </summary>
    protected new StrictEmptyActionResult Created() => new(base.Created());

    /// <summary>
    /// Wrapper around the base Created method, but returns an <see cref="ActionResult{T}"/>,
    /// to ensure the declared return type matches the actual returned value's type
    /// </summary>
    protected ActionResult<T> Created<T>(string? uri, T value) => base.Created(uri, value);

    /// <summary>
    /// Wrapper around the base NoContent method, but returns a wrapped
    /// to ensure the declared return type matches the actual returned value's type
    /// </summary>
    protected new StrictEmptyActionResult NoContent() => new(base.NoContent());
}
