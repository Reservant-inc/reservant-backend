using System.Diagnostics.CodeAnalysis;
using Reservant.ErrorCodeDocs.Attributes;

namespace ErrorCodeDocs.Test.SampleData;

/// <summary>
/// Sample methods that are used to test error code attributes
/// </summary>
[SuppressMessage("ReSharper", "EntityNameCapturedOnly.Global")]
public class SampleMethods
{
    [ErrorCode(null, ErrorCodes.FirstCode)]
    [ErrorCode(nameof(param), ErrorCodes.SecondCode, "Description")]
    public void BasicErrorCodes(string param)
    {
        throw new NotSupportedException();
    }

    [ErrorCode(nameof(InheritsFromAnotherMethod), ErrorCodes.FirstCode)]
    [ErrorCode(nameof(InheritsFromAnotherMethod), ErrorCodes.SecondCode)]
    [MethodErrorCodes<SampleMethods>(nameof(MethodToInheritFrom))]
    public void InheritsFromAnotherMethod()
    {
        throw new NotSupportedException();
    }

    [ErrorCode(nameof(MethodToInheritFrom), ErrorCodes.FirstCode)]
    [ErrorCode(nameof(MethodToInheritFrom), ErrorCodes.SecondCode)]
    public void MethodToInheritFrom()
    {
        throw new NotSupportedException();
    }

    [ErrorCode(null, ErrorCodes.FirstCode)]
    [ErrorCode(null, ErrorCodes.SecondCode)]
    [ValidatorErrorCodes<SampleDto>]
    public void InheritsFromValidator()
    {
        throw new NotSupportedException();
    }
}
