namespace JLib.ValueTypes.Implementations.FileSystem;

/// <summary> 
/// any valid filename with path information and file extension<br/>
/// Validation may differ between operating systems due to the usage of <see cref="Path.GetInvalidFileNameChars"/><br/>
/// must not contain <see cref="Path.GetInvalidFileNameChars"/><br/>
/// </summary>
/// <param name="Value">the filename</param>
public record FileNameWithoutExtension(string Value) : StringValueType(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .NotContainInvalidFileNameChars()
            .HaveNoExtension();

    /// <summary>
    /// appends the <paramref name="extension"/> to the <paramref name="name"/>. If the extension is empty, the name is returned as is.
    /// </summary>
    public static FileNameWithExtension operator +(FileNameWithoutExtension name, FileExtension extension)
        => new(
            extension.Value == ""
                ? name.Value
                : $"{name.Value}.{extension.Value}"
        );
}