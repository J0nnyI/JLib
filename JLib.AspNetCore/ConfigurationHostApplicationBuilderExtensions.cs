using JLib.Configuration;
using Microsoft.Extensions.Hosting;
using ITypeCache = JLib.Reflection.ITypeCache;

namespace JLib.AspNetCore;

/// <summary>
/// <see cref="IHostApplicationBuilder"/> extension methods for JLib.Configuration.
/// </summary>
public static class ConfigurationHostApplicationBuilderExtensions
{
    /// <summary>
    /// discovers every config section marked with a <see cref="ConfigSectionNameAttribute"/> via the <paramref name="typeCache"/>
    /// and binds it against the builders <see cref="IHostApplicationBuilder.Configuration"/>, registering the result in its <see cref="IHostApplicationBuilder.Services"/>.
    /// <br/>convenience wrapper around <see cref="ConfigurationServiceCollectionExtensions.AddAllConfigSections"/> that pulls configuration and services from the builder.
    /// </summary>
    public static TBuilder AddAllConfigSections<TBuilder>(this TBuilder builder, ITypeCache typeCache)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddAllConfigSections(typeCache, builder.Configuration);
        return builder;
    }
}
