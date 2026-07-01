using FluentAssertions;
using JLib.Configuration;
using JLib.Exceptions;
using JLib.Reflection.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace JLib.AspNetCore.Tests;

public class ConfigurationHostApplicationBuilderTests
{
    [ConfigSectionName("Demo")]
    public class DemoConfig
    {
        public string? ConfigProperty { get; init; }
    }

    [Fact]
    public void AddAllConfigSections_BindsAgainstBuilderConfiguration()
    {
        using var exceptions = new ExceptionBuilder(nameof(AddAllConfigSections_BindsAgainstBuilderConfiguration));

        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "Demo:ConfigProperty", "value1" }
        });
        builder.Services.AddTypeCache(out var typeCache, exceptions, new LoggerFactory(), "JLib.");

        builder.AddAllConfigSections(typeCache);

        using var host = builder.Build();
        host.Services.GetRequiredService<IOptions<DemoConfig>>().Value
            .ConfigProperty.Should().Be("value1");
    }
}
