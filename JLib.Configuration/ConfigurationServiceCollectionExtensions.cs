using System.Reflection;
using JLib.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ITypeCache = JLib.Reflection.ITypeCache;

namespace JLib.Configuration;
/// <summary>
/// <see cref="IServiceCollection"/> extension methods to bind config sections marked with the <see cref="ConfigSectionNameAttribute"/> to the options system.
/// </summary>
public static class ConfigurationServiceCollectionExtensions
{
    private static readonly MethodInfo ConfigureMethod =
        typeof(OptionsConfigurationServiceCollectionExtensions)
            .GetMethod(nameof(OptionsConfigurationServiceCollectionExtensions.Configure),
                [typeof(IServiceCollection), typeof(IConfiguration)]
            )
        ?? throw new InvalidSetupException("Configure method not found");

    /// <summary>
    /// binds the config section <typeparamref name="T"/> to the key declared by its <see cref="ConfigSectionNameAttribute"/>
    /// via <see cref="OptionsConfigurationServiceCollectionExtensions.Configure{TOptions}(IServiceCollection,IConfiguration)"/>.
    /// <br/>the section becomes available through the standard options interfaces (<see cref="IOptions{TOptions}"/>,
    /// <see cref="IOptionsSnapshot{TOptions}"/>, <see cref="IOptionsMonitor{TOptions}"/>), letting the consumer pick whichever fits.
    /// </summary>
    /// <exception cref="InvalidSetupException">when <typeparamref name="T"/> is missing the <see cref="ConfigSectionNameAttribute"/></exception>
    public static IServiceCollection AddConfigSection<T>(this IServiceCollection services, IConfiguration config)
        where T : class
    {
        var sectionName = ConfigSectionNameAttribute.ResolveSectionName(typeof(T));
        services.Configure<T>(config.GetSection(sectionName.Value));
        return services;
    }

    /// <summary>
    /// binds the config section <paramref name="sectionType"/> to the key declared by its <see cref="ConfigSectionNameAttribute"/>.
    /// <br/>non-generic counterpart of <see cref="AddConfigSection{T}"/> for binding section types that are only known at runtime.
    /// </summary>
    /// <exception cref="InvalidSetupException">when <paramref name="sectionType"/> is missing the <see cref="ConfigSectionNameAttribute"/></exception>
    public static IServiceCollection AddConfigSection(this IServiceCollection services, Type sectionType, IConfiguration config)
    {
        var sectionName = ConfigSectionNameAttribute.ResolveSectionName(sectionType);
        ConfigureMethod.MakeGenericMethod(sectionType)
            .Invoke(null, [services, config.GetSection(sectionName.Value)]);
        return services;
    }

    /// <summary>
    /// discovers every <see cref="ConfigurationSectionType"/> via the <paramref name="typeCache"/> and binds each one
    /// through <see cref="AddConfigSection(IServiceCollection,Type,IConfiguration)"/>, so config sections do not have to be registered individually.
    /// </summary>
    public static IServiceCollection AddAllConfigSections(this IServiceCollection services,
        ITypeCache typeCache, IConfiguration config)
    {
        foreach (var sectionType in typeCache.All<ConfigurationSectionType>())
            services.AddConfigSection(sectionType.Value, config);

        return services;
    }
}
