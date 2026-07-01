using FluentAssertions;
using JLib.Exceptions;
using JLib.Reflection;
using JLib.Reflection.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace JLib.Configuration.Tests;

public class ConfigurationTests(ITestOutputHelper testOutputHelper)
{
    [ConfigSectionName("Demo")]
    public class DemoConfig
    {
        public string? ConfigProperty { get; init; }
    }

    [ConfigSectionName("Secondary")]
    public class SecondaryConfig
    {
        public string? Name { get; init; }
    }

    public class NoAttributeConfig
    {
        public string? Value { get; init; }
    }

    #region helpers

    private static IConfiguration BuildConfig(params Dictionary<string, string?>[] sources)
    {
        var builder = new ConfigurationBuilder();
        foreach (var source in sources)
            builder.AddInMemoryCollection(source);
        return builder.Build();
    }

    private static IConfiguration BuildConfig(string key, string? value)
        => BuildConfig(new Dictionary<string, string?> { { key, value } });

    /// <summary>
    /// builds a type package containing the <see cref="ConfigurationSectionType"/> factory plus the nested
    /// config sections of <paramref name="sectionContainer"/>. scoping discovery to an explicit container
    /// (instead of scanning the whole assembly) keeps deliberately-invalid fixtures from leaking between tests.
    /// </summary>
    private static ITypePackage SectionPackage(ILoggerFactory logger, Type sectionContainer)
        => new TypePackageBuilder(logger)
            .Add<ConfigurationSectionType>()
            .AddNestedTypes(sectionContainer)
            .Build();

    private void WithAllSections(IConfiguration config, Action<IServiceProvider> assert)
    {
        using var exceptions = new ExceptionBuilder(nameof(WithAllSections));
        var logger = new LoggerFactory().AddXunit(testOutputHelper);
        var package = SectionPackage(logger, typeof(ConfigurationTests));

        using var serviceProvider = new ServiceCollection()
            .AddTypeCache(out var typeCache, exceptions, logger, package)
            .AddAllConfigSections(typeCache, config)
            .BuildServiceProvider();

        assert(serviceProvider);
    }

    #endregion

    #region AddConfigSection<T>

    [Fact]
    public void AddConfigSection_BindsViaAttribute()
    {
        using var serviceProvider = new ServiceCollection()
            .AddConfigSection<DemoConfig>(BuildConfig("Demo:ConfigProperty", "value1"))
            .BuildServiceProvider();

        serviceProvider.GetRequiredService<IOptions<DemoConfig>>().Value
            .ConfigProperty.Should().Be("value1");
    }

    [Fact]
    public void AddConfigSection_WithoutAttribute_Throws()
    {
        var act = () => new ServiceCollection().AddConfigSection<NoAttributeConfig>(BuildConfig());

        act.Should().Throw<InvalidSetupException>()
            .WithMessage($"*{nameof(ConfigSectionNameAttribute)}*");
    }

    [Fact]
    public void AddConfigSection_ExposesAllOptionsInterfaces()
    {
        using var serviceProvider = new ServiceCollection()
            .AddConfigSection<DemoConfig>(BuildConfig("Demo:ConfigProperty", "value1"))
            .BuildServiceProvider();

        serviceProvider.GetRequiredService<IOptions<DemoConfig>>().Value
            .ConfigProperty.Should().Be("value1");
        serviceProvider.GetRequiredService<IOptionsMonitor<DemoConfig>>().CurrentValue
            .ConfigProperty.Should().Be("value1");

        using var scope = serviceProvider.CreateScope();
        scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<DemoConfig>>().Value
            .ConfigProperty.Should().Be("value1");
    }

    [Fact]
    public void AddConfigSection_NonGeneric_BindsViaAttribute()
    {
        using var serviceProvider = new ServiceCollection()
            .AddConfigSection(typeof(DemoConfig), BuildConfig("Demo:ConfigProperty", "value1"))
            .BuildServiceProvider();

        serviceProvider.GetRequiredService<IOptions<DemoConfig>>().Value
            .ConfigProperty.Should().Be("value1");
    }

    [Fact]
    public void AddConfigSection_NonGeneric_WithoutAttribute_Throws()
    {
        var act = () => new ServiceCollection().AddConfigSection(typeof(NoAttributeConfig), BuildConfig());

        act.Should().Throw<InvalidSetupException>()
            .WithMessage($"*{nameof(ConfigSectionNameAttribute)}*");
    }

    [Fact]
    public void AddConfigSection_AbsentSection_BindsDefaultInstance()
    {
        using var serviceProvider = new ServiceCollection()
            .AddConfigSection<DemoConfig>(BuildConfig("Unrelated:Key", "value"))
            .BuildServiceProvider();

        var value = serviceProvider.GetRequiredService<IOptions<DemoConfig>>().Value;
        value.Should().NotBeNull();
        value.ConfigProperty.Should().BeNull();
    }

    #endregion

    #region AddAllConfigSections

    [Fact]
    public void AddAllConfigSections_BindsDiscoveredSection()
        => WithAllSections(BuildConfig("Demo:ConfigProperty", "value1"), provider =>
            provider.GetRequiredService<IOptions<DemoConfig>>().Value
                .ConfigProperty.Should().Be("value1"));

    [Fact]
    public void AddAllConfigSections_BindsMultipleSections()
        => WithAllSections(BuildConfig(new Dictionary<string, string?>
        {
            { "Demo:ConfigProperty", "demoValue" },
            { "Secondary:Name", "secondaryValue" }
        }), provider =>
        {
            provider.GetRequiredService<IOptions<DemoConfig>>().Value
                .ConfigProperty.Should().Be("demoValue");
            provider.GetRequiredService<IOptions<SecondaryConfig>>().Value
                .Name.Should().Be("secondaryValue");
        });

    [Fact]
    public void AddAllConfigSections_LaterSourcesOverrideEarlierOnes()
        => WithAllSections(BuildConfig(
            new Dictionary<string, string?> { { "Demo:ConfigProperty", "value1" } },
            new Dictionary<string, string?> { { "Demo:ConfigProperty", "value2" } }), provider =>
            provider.GetRequiredService<IOptions<DemoConfig>>().Value
                .ConfigProperty.Should().Be("value2"));

    [Fact]
    public void GenericConfigSection_IsFlaggedInvalidDuringTypeCacheBuild()
    {
        // an open generic [ConfigSectionName] type is discovered but must be rejected by validation,
        // surfacing a build-time error instead of crashing later in AddAllConfigSections.
        var logger = new LoggerFactory().AddXunit(testOutputHelper);
        var package = SectionPackage(logger, typeof(GenericConfigSectionContainer));
        var exceptions = new ExceptionBuilder(nameof(GenericConfigSection_IsFlaggedInvalidDuringTypeCacheBuild));

        new ServiceCollection().AddTypeCache(out _, exceptions, logger, package);

        exceptions.HasErrors().Should().BeTrue();
        exceptions.GetException()!.ToString().Should().Contain("Generic");
    }

    #endregion

    #region ConfigurationHelper

    [Fact]
    public void GetSection_ReturnsSectionFromAttribute()
    {
        var section = BuildConfig("Demo:ConfigProperty", "value1")
            .GetSection<DemoConfig>(new LoggerFactory());

        section.Key.Should().Be("Demo");
        section["ConfigProperty"].Should().Be("value1");
    }

    [Fact]
    public void GetSection_WithoutAttribute_Throws()
    {
        var act = () => BuildConfig().GetSection<NoAttributeConfig>(new LoggerFactory());

        act.Should().Throw<InvalidSetupException>()
            .WithMessage($"*{nameof(ConfigSectionNameAttribute)}*");
    }

    [Fact]
    public void GetSectionObject_BindsInstance()
    {
        var instance = BuildConfig("Demo:ConfigProperty", "value1")
            .GetSectionObject<DemoConfig>(new LoggerFactory());

        instance.ConfigProperty.Should().Be("value1");
    }

    [Fact]
    public void GetSectionObject_AbsentSection_ReturnsDefaultInstance()
    {
        var instance = BuildConfig("Unrelated:Key", "value")
            .GetSectionObject<DemoConfig>(new LoggerFactory());

        instance.Should().NotBeNull();
        instance.ConfigProperty.Should().BeNull();
    }

    [Fact]
    public void GetSectionObject_WithoutAttribute_Throws()
    {
        var act = () => BuildConfig().GetSectionObject<NoAttributeConfig>(new LoggerFactory());

        act.Should().Throw<InvalidSetupException>()
            .WithMessage($"*{nameof(ConfigSectionNameAttribute)}*");
    }

    #endregion
}

/// <summary>
/// holds a deliberately invalid (open generic) config section in its own top-level container, so it is
/// only seen by the validation test and never leaks into the discovery used by the other tests.
/// </summary>
internal static class GenericConfigSectionContainer
{
    [ConfigSectionName("Generic")]
    public class GenericConfig<T>
    {
        public string? Value { get; init; }
    }
}
