namespace JLib.ValueTypes.Implementations.FileSystem;

/// <summary>
/// the name of a single directory in a path, not a <see cref="RelativeDirectoryPath"/>, <see cref="AbsoluteDirectoryPath"/> or <see cref="DriveLetter"/><br/><br/>
/// Validation may differ between operating systems due to the usage of <see cref="Path.GetInvalidPathChars"/><br/><br/>
/// must not contain <see cref="Path.GetInvalidPathChars"/><br/>
/// must not contain <see cref="Path.DirectorySeparatorChar"/><br/>
/// must not contain <see cref="Path.AltDirectorySeparatorChar"/><br/>
/// </summary>
/// <param name="Value">the directory name</param>
public record DirectoryName(string Value) : StringValueType(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .NotContain(Path.GetInvalidPathChars())
            .NotContain(Path.DirectorySeparatorChar)
            .NotContain(Path.AltDirectorySeparatorChar);

    /// <returns><paramref name="dir1"/> and <paramref name="dir2"/> combined into a <see cref="RelativeDirectoryPath"/></returns>
    public static RelativeDirectoryPath operator +(DirectoryName dir1, DirectoryName dir2)
        => new(Path.Combine(dir1.Value, dir2.Value));
}