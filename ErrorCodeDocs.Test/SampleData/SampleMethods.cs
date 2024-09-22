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
}
