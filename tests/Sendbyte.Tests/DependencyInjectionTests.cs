using Microsoft.Extensions.DependencyInjection;
using Sendbyte.DependencyInjection;
using Xunit;

namespace Sendbyte.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddSendbyte_RegistersISendbyteClient()
    {
        var services = new ServiceCollection();

        services.AddSendbyte(options =>
        {
            options.ApiKey = "sk_test_123";
        });

        using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<ISendbyteClient>();

        Assert.NotNull(client);
        Assert.NotNull(client.Emails);
        Assert.NotNull(client.Domains);
        Assert.NotNull(client.Webhooks);
    }

    [Fact]
    public void AddSendbyte_ThrowsUsefulError_WhenApiKeyIsMissing()
    {
        var services = new ServiceCollection();

        services.AddSendbyte(options =>
        {
            options.ApiKey = "";
        });

        using var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            provider.GetRequiredService<ISendbyteClient>());

        Assert.Contains("API key is required", exception.Message);
    }
}
