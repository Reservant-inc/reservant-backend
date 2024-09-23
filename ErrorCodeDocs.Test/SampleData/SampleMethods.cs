using System.Diagnostics.CodeAnalysis;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.ErrorCodeDocs.Test.SampleData;

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
    [MethodErrorCodes<SampleMethods>(nameof(FirstOrderInheritedMethod))]
    [MethodErrorCodes(typeof(SampleMethods), nameof(InheritedUsingTypeof))]
    public void InheritsFromAnotherMethod()
    {
        throw new NotSupportedException();
    }

    [ErrorCode(nameof(FirstOrderInheritedMethod), ErrorCodes.FirstCode)]
    [ErrorCode(nameof(FirstOrderInheritedMethod), ErrorCodes.SecondCode)]
    [MethodErrorCodes<SampleMethods>(nameof(SecondOrderInheritedMethod))]
    public void FirstOrderInheritedMethod()
    {
        throw new NotSupportedException();
    }

    [ErrorCode(nameof(InheritedUsingTypeof), ErrorCodes.FirstCode)]
    public void InheritedUsingTypeof()
    {
        throw new NotSupportedException();
    }

    [ErrorCode(nameof(SecondOrderInheritedMethod), ErrorCodes.FirstCode)]
    public void SecondOrderInheritedMethod()
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

    [ErrorCode(null, ErrorCodes.FirstCode)]
    [MethodErrorCodes<SampleMethods>(nameof(ReferenceCycle2))]
    public void ReferenceCycle()
    {
        throw new NotSupportedException();
    }

    [ErrorCode(null, ErrorCodes.SecondCode)]
    [MethodErrorCodes<SampleMethods>(nameof(ReferenceCycle))]
    public void ReferenceCycle2()
    {
        throw new NotSupportedException();
    }

    [ErrorCode(nameof(SampleDto.Property1), ErrorCodes.ValidatorFirstCode, "Description from validator")]
    [ErrorCode(nameof(SampleDto.Property2), ErrorCodes.ValidatorSecondCode, "Different description")]
    [ErrorCode(nameof(FirstOrderInheritedMethod), ErrorCodes.FirstCode)]
    [ErrorCode(nameof(FirstOrderInheritedMethod), ErrorCodes.SecondCode, "Different description")]
    [MethodErrorCodes<SampleMethods>(nameof(FirstOrderInheritedMethod))]
    [ValidatorErrorCodes<SampleDto>]
    public void DuplicateCodes()
    {
        throw new NotSupportedException();
    }
}
