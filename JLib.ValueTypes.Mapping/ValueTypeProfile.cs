using System.Linq.Expressions;
using System.Reflection;

using AutoMapper;

using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;

using Microsoft.Extensions.Logging;

namespace JLib.ValueTypes.Mapping;

/// <summary>
/// Provides Mappings for all value types.<br/>
/// to enable this, an ExpressionVisitor is used which replaces a temporary placeholder with the valid constructor.
/// this is required, since we can not call a ctor with parameters of a generic type argument
/// </summary>
public class ValueTypeProfile : Profile
{
    private static class ClassValueTypeConversions<TValueType, TNative>
        where TValueType : ValueType<TNative>
        where TNative : class
    {
        public static void AddMapping(Profile profile, ILogger<ValueTypeProfile> logger)
        {
            logger.LogTrace("            {tvt}? => {tNative}?", typeof(TValueType).Name,
                typeof(TNative).Name);
            profile.CreateMap<TValueType?, TNative?>().ConvertUsing(vt => vt == null ? null : vt.Value);
            logger.LogTrace("            {tNative}? => {tvt}?", typeof(TNative).Name,
                typeof(TValueType).Name);
            profile.CreateMap<TNative?, TValueType?>().ConvertUsing(
                ValueType.FactoryExpressions.ForNullableClass<TValueType, TNative>());
        }
    }

    private static class StructValueTypeConversions<TValueType, TNative>
        where TValueType : ValueType<TNative>
        where TNative : struct
    {
        public static void AddMapping(Profile profile, ILogger<ValueTypeProfile> logger)
        {
            logger.LogTrace("            {tvt} => {tNative}", typeof(TValueType).Name,
                typeof(TNative).Name);
            profile.CreateMap<TValueType, TNative>().ConvertUsing(vt => vt.Value);


            logger.LogTrace("            {tNative} => {tvt}", typeof(TNative).Name,
                typeof(TValueType).Name);
            profile.CreateMap<TNative, TValueType>().ConvertUsing(
                ValueType.FactoryExpressions.ForNonNullableStruct<TValueType, TNative>());


            logger.LogTrace("            {tvt}? => {tNative}?", typeof(TValueType).Name,
                typeof(TNative?).FullName());
            profile.CreateMap<TValueType, TNative?>().ConvertUsing(vt => vt == null ? null : vt.Value);


            logger.LogTrace("            {tNative}? => {tvt}?",
                typeof(TNative?).FullName(), typeof(TValueType).Name);

            profile.CreateMap<TNative?, TValueType?>()
                .ConvertUsing(ValueType.FactoryExpressions.ForNullableStruct<TValueType, TNative>());
        }
    }

    /// <summary>
    /// <inheritdoc cref="ValueTypeProfile"/>
    /// </summary>
    public ValueTypeProfile(ITypeCache cache, ILogger<ValueTypeProfile> logger)
    {
        using var exceptions = new ExceptionBuilder(nameof(ValueTypeProfile));
        foreach (var valueType in cache.All<ValueTypeType>().Where(vt => vt is { DisableAutomatedProfileGeneration: false, Value.IsAbstract: false }))
        {
            try
            {
                if (valueType.NativeType.IsClass)
                {
                    logger.LogDebug("        adding map for class-valueType {valueType}", valueType.Name);

                    var addMapping = typeof(ClassValueTypeConversions<,>)
                                         .MakeGenericType(valueType.Value, valueType.NativeType)
                                         .GetMethod(nameof(ClassValueTypeConversions<ValueType<Ignored>, Ignored>.AddMapping)) ??
                                     throw new InvalidSetupException("AddProfileMethodNotFound");

                    addMapping.Invoke(null, new object?[] { this, logger });

                }
                else
                {
                    logger.LogDebug("        adding map for struct-valueType {valueType}", valueType.Name);
                    var addMapping = typeof(StructValueTypeConversions<,>)
                                         .MakeGenericType(valueType.Value, valueType.NativeType)
                                         .GetMethod(nameof(StructValueTypeConversions<ValueType<int>, int>.AddMapping)) ??
                                     throw new InvalidSetupException("AddProfileMethodNotFound");

                    addMapping.Invoke(null, new object?[] { this, logger });
                }

            }
            catch (Exception e)
            {
                exceptions.Add(new Exception($"failed to map {valueType.Value.FullName()}", e));
            }
        }
    }
}