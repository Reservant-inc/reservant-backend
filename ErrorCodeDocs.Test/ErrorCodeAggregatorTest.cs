using System.Reflection;
using ErrorCodeDocs.Test.SampleData;
using Reservant.ErrorCodeDocs;

namespace ErrorCodeDocs.Test;

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

    private static MethodInfo GetSampleMethod(string name, params Type[] paramTypes)
        => typeof(SampleMethods).GetMethod(name, paramTypes)
           ?? throw new InvalidOperationException(
               $"Method '{name}' with parameters [{string.Join(", ", paramTypes.Select(t => t.Name))}] not found");
}
