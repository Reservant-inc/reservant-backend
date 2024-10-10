using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Validation;

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
    [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates",
        Justification = "Designed to be invisible")]
    [SuppressMessage("Design", "CA1034:Nested types should not be visible")]
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
    protected ActionResult<T> Created<T>(Uri? uri, T value) => base.Created(uri, value);

    /// <summary>
    /// Wrapper around the base NoContent method, but returns a wrapped
    /// to ensure the declared return type matches the actual returned value's type
    /// </summary>
    protected new StrictEmptyActionResult NoContent() => new(base.NoContent());

    /// <summary>
    /// Create an ActionResult from a result
    /// </summary>
    /// <remarks>
    /// 200 OK with the result's value if the result is successful<br/>
    /// 400 BAD REQUEST if there are validation errors
    /// </remarks>
    protected ActionResult<T> OkOrErrors<T>(Result<T> result)
    {
        if (result.IsError)
        {
            var problemDetails = ((CustomProblemDetailsFactory)ProblemDetailsFactory)
                .CreateFluentValidationProblemDetails(result.Errors);
            return new ObjectResult(problemDetails);
        }

        return base.Ok(result.Value);
    }

    /// <summary>
    /// Create an OkResult from a result
    /// </summary>
    /// <remarks>
    /// 204 NO CONTENT if the result is successful<br/>
    /// 400 BAD REQUEST if there are validation errors
    /// </remarks>
    protected StrictEmptyActionResult OkOrErrors(Result result)
    {
        if (result.IsError)
        {
            var problemDetails = ((CustomProblemDetailsFactory)ProblemDetailsFactory)
                .CreateFluentValidationProblemDetails(result.Errors);
            return new StrictEmptyActionResult(new ObjectResult(problemDetails));
        }

        return new StrictEmptyActionResult(base.NoContent());
    }
}
