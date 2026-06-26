using Sendbyte.Configuration;
using Xunit;

namespace Sendbyte.Tests;

public class SendbyteOptionsTests
{
    [Fact]
    public void DefaultBaseUrl_IsSendbyteApiBaseUrl()
    {
        var options = new SendbyteOptions();

        Assert.Equal("https://api.sendbyte.africa/v1", options.BaseUrl);
    }
}
