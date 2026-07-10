using NotificationService.Domain;

namespace NotificationService.Tests;

public class ChannelTypeTests
{
    [Fact]
    public void Email_ReturnsExpectedValue()
    {
        Assert.Equal("email", ChannelType.Email);
    }

    [Fact]
    public void Push_ReturnsExpectedValue()
    {
        Assert.Equal("push", ChannelType.Push);
    }

    [Fact]
    public void EmailAndPush_AreDifferent()
    {
        Assert.NotEqual(ChannelType.Email, ChannelType.Push);
    }
}
