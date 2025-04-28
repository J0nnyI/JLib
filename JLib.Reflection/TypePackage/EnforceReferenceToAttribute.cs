namespace JLib.Reflection;

/// <summary>
/// This Attribute forces a reference to an Assembly, the types of which may not be referenced otherwise by this assembly.<br/>
/// This is required, when the referencing assembly does use the types of the referenced assembly for reflection but does not reference them directly.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class EnforceReferenceToAttribute : Attribute
{

    /// <summary>
    /// <inheritdoc cref="EnforceReferenceToAttribute"/>
    /// </summary>
    /// <param name="type">The Type, which is defined by the assembly </param>
    // ReSharper disable once UnusedParameter.Local
    public EnforceReferenceToAttribute(params Type[] type)
    {
    }
}