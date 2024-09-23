using System.Reflection;
using Reservant.ErrorCodeDocs.Test.SampleData;

namespace Reservant.ErrorCodeDocs.Test;

public class ErrorCodeAggregatorTest
{
    private readonly ErrorCodesAggregator _sut = new(typeof(ErrorCodeAggregatorTest).Assembly);

    [Fact]
    public void BasicCodes()
    {
        // Arrange
        var method = GetSampleMethod(nameof(SampleMethods.BasicErrorCodes), typeof(string));

        // Act
        var codes = _sut.GetErrorCodes(method);

        // Assert
        var expected = new ErrorCodeDescription[]
        {
            new(null, ErrorCodes.FirstCode),
            new(method.GetParameters()[0].Name, ErrorCodes.SecondCode, "Description"),
        };
        Assert.Equal(expected, codes);
    }

    [Fact]
    public void InheritedFromAnotherMethod()
    {
        // Arrange
        var method = GetSampleMethod(nameof(SampleMethods.InheritsFromAnotherMethod));

        // Act
        var codes = _sut.GetErrorCodes(method);

        // Assert
        var expected = new ErrorCodeDescription[]
        {
            new(nameof(SampleMethods.FirstOrderInheritedMethod), ErrorCodes.FirstCode),
            new(nameof(SampleMethods.FirstOrderInheritedMethod), ErrorCodes.SecondCode),
            new(nameof(SampleMethods.InheritedUsingTypeof), ErrorCodes.FirstCode),
            new(method.Name, ErrorCodes.FirstCode),
            new(method.Name, ErrorCodes.SecondCode),
            new(nameof(SampleMethods.SecondOrderInheritedMethod), ErrorCodes.FirstCode),
        };
        Assert.Equal(expected, codes);
    }

    [Fact]
    public void InheritedFromValidator()
    {
        // Arrange
        var method = GetSampleMethod(nameof(SampleMethods.InheritsFromValidator));

        // Act
        var codes = _sut.GetErrorCodes(method);

        // Assert
        var expected = new ErrorCodeDescription[]
        {
            new(null, ErrorCodes.FirstCode),
            new(null, ErrorCodes.SecondCode),
            new(nameof(SampleDto.Property1), ErrorCodes.ValidatorFirstCode, "Description from validator"),
            new(nameof(SampleDto.Property2), ErrorCodes.ValidatorSecondCode, "Description from validator"),
        };
        Assert.Equal(expected, codes);
    }

    [Fact]
    public void ReferenceCyclesAreIgnored()
    {
        // Arrange
        var method = GetSampleMethod(nameof(SampleMethods.ReferenceCycle));

        // Act
        var codes = _sut.GetErrorCodes(method);

        // Assert
        var expected = new ErrorCodeDescription[]
        {
            new(null, ErrorCodes.FirstCode),
            new(null, ErrorCodes.SecondCode),
        };
        Assert.Equal(expected, codes);
    }

    [Fact]
    public void RemovesDuplicates()
    {
        // Arrange
        var method = GetSampleMethod(nameof(SampleMethods.DuplicateCodes));

        // Act
        var codes = _sut.GetErrorCodes(method);

        // Assert
        var expected = new ErrorCodeDescription[]
        {
            new(nameof(SampleMethods.FirstOrderInheritedMethod), ErrorCodes.FirstCode),
            new(nameof(SampleMethods.FirstOrderInheritedMethod), ErrorCodes.SecondCode),
            new(nameof(SampleMethods.FirstOrderInheritedMethod), ErrorCodes.SecondCode, "Different description"),
            new(nameof(SampleDto.Property1), ErrorCodes.ValidatorFirstCode, "Description from validator"),
            new(nameof(SampleDto.Property2), ErrorCodes.ValidatorSecondCode, "Description from validator"),
            new(nameof(SampleDto.Property2), ErrorCodes.ValidatorSecondCode, "Different description"),
            new(nameof(SampleMethods.SecondOrderInheritedMethod), ErrorCodes.FirstCode),
        };
        Assert.Equal(expected, codes);
    }

    private static MethodInfo GetSampleMethod(string name, params Type[] paramTypes)
        => typeof(SampleMethods).GetMethod(name, paramTypes)
           ?? throw new InvalidOperationException(
               $"Method '{name}' with parameters [{string.Join(", ", paramTypes.Select(t => t.Name))}] not found");
}
