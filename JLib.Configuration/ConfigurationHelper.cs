using JLib.Exceptions;
using JLib.Helper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JLib.Configuration;

/// <summary>
/// Helper functions enabling the access of config sections which are marked with the <see cref="ConfigSectionNameAttribute"/>
/// </summary>
public static class ConfigurationHelper
{
    /// <summary>
    /// returns the section <typeparamref name="T"/> under the key defined by its <see cref="ConfigSectionNameAttribute"/>.
    /// if the attribute is not found, a <see cref="InvalidSetupException"/> will be thrown
    /// <br/>does not validate
    /// <br/>does not check whether the section is actually present
    /// </summary>
    public static IConfigurationSection GetSection<T>(this IConfiguration config, ILoggerFactory loggerFactory)
        where T : class, new()
    {
        var sectionName = ConfigSectionNameAttribute.ResolveSectionName(typeof(T));

        var logger = loggerFactory.CreateLogger<T>();
        logger.LogInformation("Loading section {section} ({sectionType})", sectionName.Value,
            typeof(T).FullName(true));

        return config.GetSection(sectionName.Value);
    }

    /// <summary>
    /// returns a new instance of the section <typeparamref name="T"/>, bound from the key defined by its <see cref="ConfigSectionNameAttribute"/>.
    /// <inheritdoc cref="GetSection{T}(IConfiguration,ILoggerFactory)"/>
    /// </summary>
    public static T GetSectionObject<T>(this IConfiguration config, ILoggerFactory loggerFactory)
        where T : class, new()
    {
        var instance = new T();
        var section = config.GetSection<T>(loggerFactory);
        section.Bind(instance);
        return instance;
    }
}