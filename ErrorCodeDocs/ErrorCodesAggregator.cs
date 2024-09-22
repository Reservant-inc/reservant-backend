using FluentValidation;
using System.Reflection;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.ErrorCodeDocs;

/// <summary>
/// Methods to aggregate error codes for a selected method
/// </summary>
public class ErrorCodesAggregator
{
    private static readonly Comparer<ErrorCodeDescription> ErrorCodeComparer =
        Comparer<ErrorCodeDescription>.Default;

    private readonly Dictionary<Type, Type> _validators = [];

    /// <summary>
    /// Construct the object with a given assembly
    /// </summary>
    /// <param name="getValidatorsFromAssembly">Assembly to get validators from</param>
    public ErrorCodesAggregator(Assembly getValidatorsFromAssembly)
    {
        var validators = AssemblyScanner.FindValidatorsInAssembly(getValidatorsFromAssembly);
        foreach (var validator in validators)
        {
            var validatedType = validator.InterfaceType.GetGenericArguments()[0];
            var validatorType = validator.ValidatorType;
            _validators.Add(validatedType, validatorType);
        }
    }

    /// <summary>
    /// Get error codes that can be returned from a method
    /// </summary>
    /// <param name="method">The method</param>
    public IEnumerable<ErrorCodeDescription> GetErrorCodes(MethodInfo method)
    {
        var codes = new SortedSet<ErrorCodeDescription>(ErrorCodeComparer);

        foreach (var errorCode in method.GetCustomAttributes<ErrorCodeAttribute>())
        {
            codes.Add(new ErrorCodeDescription(errorCode));
        }

        foreach (var inheritedMethod in method.GetCustomAttributes(typeof(MethodErrorCodesAttribute<>)))
        {
            foreach (var errorCode in GetReferencedMethodErrorCodes(inheritedMethod))
            {
                codes.Add(errorCode);
            }
        }

        foreach (var inheritedValidator in method.GetCustomAttributes(typeof(ValidatorErrorCodesAttribute<>)))
        {
            foreach (var errorCode in GetValidatorErrorCodes(inheritedValidator))
            {
                codes.Add(errorCode);
            }
        }

        return codes;
    }

    /// <summary>
    /// Get error codes that can be returned from the method referenced in a <see cref="MethodErrorCodesAttribute{TContaining}"/>
    /// </summary>
    /// <param name="attribute">The attribute, must inherit from <see cref="MethodErrorCodesAttribute"/></param>
    /// <exception cref="ArgumentException">The attribute does not inherit from <see cref="MethodErrorCodesAttribute"/></exception>
    /// <exception cref="InvalidOperationException">The method referenced was not found</exception>
    private IEnumerable<ErrorCodeDescription> GetReferencedMethodErrorCodes(Attribute attribute)
    {
        if (attribute is not MethodErrorCodesAttribute methodAttribute)
        {
            throw new ArgumentException($"Attribute must be a {nameof(MethodErrorCodesAttribute)}", nameof(attribute));
        }

        var referencedMethod = methodAttribute.ContainingType
            .GetMethod(methodAttribute.MethodName)
            ?? throw new InvalidOperationException(
                $"Method not found {methodAttribute.MethodName} in type {methodAttribute.ContainingType.Name}");

        foreach (var errorCode in GetErrorCodes(referencedMethod))
        {
            yield return errorCode;
        }
    }

    /// <summary>
    /// Get error codes that can be returned by a validator
    /// </summary>
    /// <param name="attribute">
    /// <see cref="ValidatorErrorCodesAttribute{TValidated}"/> that references
    /// the validator
    /// </param>
    /// <exception cref="ArgumentException">
    /// If the attribute is not a <see cref="ValidatorErrorCodesAttribute{TValidated}"/>
    /// </exception>
    private IEnumerable<ErrorCodeDescription> GetValidatorErrorCodes(Attribute attribute)
    {
        if (attribute.GetType().GetGenericTypeDefinition() != typeof(ValidatorErrorCodesAttribute<>))
        {
            throw new ArgumentException(
                "Attribute must be a ValidatorErrorCodesAttribute", nameof(attribute));
        }

        var validatedType = attribute.GetType().GetGenericArguments()[0];
        var validatorDescriptor = FindValidator(validatedType).CreateDescriptor();
        foreach (var rule in validatorDescriptor.Rules)
        {
            var propertyName = GetPropertyNameForRule(rule);

            foreach (var component in rule.Components)
            {
                yield return new ErrorCodeDescription
                {
                    PropertyName = propertyName,
                    ErrorCode = component.ErrorCode ?? component.Validator.Name,
                    Description = component.GetUnformattedErrorMessage(),
                };
            }
        }
    }

    /// <summary>
    /// Return property name for a validation rule as it is returned
    /// when the rule has failed
    /// </summary>
    private string GetPropertyNameForRule(IValidationRule rule)
    {
        var propertyName = rule.PropertyName;
        var isRuleForEach = rule.GetType()
            .GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollectionRule<,>));
        if (isRuleForEach)
        {
            propertyName += "[<index>]";
        }

        return propertyName;
    }

    /// <summary>
    /// Get a validator instance for a validated type
    /// </summary>
    /// <exception cref="InvalidOperationException">If validator for the type not found</exception>
    private IValidator FindValidator(Type validatedType)
    {
        if (!_validators.TryGetValue(validatedType, out var validatorType))
        {
            throw new InvalidOperationException(
                $"Validator for type {validatedType.Name} not found");
        }

        var constructor = validatorType.GetConstructors()[0];
        var numberOfParameters = constructor.GetParameters().Length;
        // We supply nulls for arguments which is OK since we only construct
        // the validator to get the rule list
        return (IValidator)constructor.Invoke(new object[numberOfParameters]);
    }
}
