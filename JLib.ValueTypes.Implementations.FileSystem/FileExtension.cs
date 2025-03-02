namespace JLib.ValueTypes.Implementations.FileSystem;

/// <summary>
/// any valid file extension<br/>
/// Validation may differ between operating systems due to the usage of <see cref="Path.GetInvalidFileNameChars"/><br/>
/// must not contain <see cref="Path.GetInvalidFileNameChars"/><br/>
/// must not start with '.'
/// </summary>
/// <param name="Value">the file extension</param>
public record FileExtension(string Value) : StringValueType(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .NotStartWith('.')
            .NotContainInvalidFileNameChars();
}