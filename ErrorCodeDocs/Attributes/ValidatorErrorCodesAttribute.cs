namespace Reservant.ErrorCodeDocs.Attributes;

/// <summary>
/// Documents that an endpoint (or a method) can return error codes
/// produced by the validator for the type <typeparamref name="TValidated"/>
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ValidatorErrorCodesAttribute<TValidated> : Attribute
{
    /// <summary>
    /// Type of the object validated
    /// </summary>
    public Type TypeValidated => typeof(TValidated);
}
