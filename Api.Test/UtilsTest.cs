using Reservant.Api;

namespace Api.Test;

public class UtilsTest
{
    [Fact]
    public void IsValidNip_Success()
    {
        Assert.True(Utils.IsValidNip("1060000062"));
    }

    [Fact]
    public void IsValidNip_InvalidLength()
    {
        Assert.False(Utils.IsValidNip("10600000622"));
    }

    [Fact]
    public void IsValidNip_InvalidCheckSum()
    {
        Assert.False(Utils.IsValidNip("1060000063"));
    }

    [Fact]
    public void IsValidNip_InvalidCharacters()
    {
        Assert.False(Utils.IsValidNip("10600000a3"));
    }
}
