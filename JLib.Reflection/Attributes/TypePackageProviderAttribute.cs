namespace JLib.Reflection;

/// <summary>
/// indicates, that the decorated type provides the type package for the assembly.<br/>
/// must only be used once per assembly
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
[Obsolete($"use the {nameof(TypePackageBuilder)} instead. it no longer need TypePackageProviders.")]
public class TypePackageProviderAttribute : Attribute { }