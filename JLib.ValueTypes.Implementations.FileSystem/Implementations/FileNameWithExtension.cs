// ReSharper disable UnusedMember.Local

namespace JLib.ValueTypes.Implementations.FileSystem;

/// <summary>
/// any valid filename without path information, but with and file extension<br/>
/// Validation may differ between operating systems due to the usage of <see cref="Path.GetInvalidFileNameChars"/><br/>
/// must not contain <see cref="Path.GetInvalidFileNameChars"/><br/>
/// must contain '.'
/// </summary>
/// <param name="Value">the filename</param>
public record FileNameWithExtension(string Value) : FileName(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .HaveAnExtension();

    /// <returns>the <see cref="FileExtension"/></returns>
    public FileExtension GetExtension()
        => new(Path.GetExtension(Value).TrimStart('.'));

    /// <returns>the <see cref="FileNameWithoutExtension"/></returns>
    public FileNameWithoutExtension RemoveExtension()
        => new(Path.GetFileNameWithoutExtension(Value));

    /// <returns>a <see cref="RelativeFilePath"/> of <paramref name="dirName"/>/<paramref name="fileName"/></returns>
    public static RelativeFilePath operator +(DirectoryName dirName, FileNameWithExtension fileName)
        => new(Path.Combine(dirName.Value, fileName.Value));

}