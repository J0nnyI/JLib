namespace JLib.ValueTypes.Implementations.FileSystem;

/// <summary>
/// The letter of a windows filesystem drive, e.g. 'C'
/// </summary>
public record DriveLetter(char Value) : CharValueType(Value)
{
    [Validation]
    // ReSharper disable once UnusedMember.Local
    private static void Validate(ValidationContext<char> must)
        => must.BeAsciiLetter();

    /// <summary>
    /// appends the <paramref name="path"/> to the <paramref name="drive"/>, making it absolute
    /// </summary>
    public static AbsoluteDirectoryPath operator +(DriveLetter drive, RelativeDirectoryPath path)
        => new(Path.Combine($"{drive.Value}:", path.Value));
    /// <summary>
    /// appends the <paramref name="filePath"/> to the <paramref name="drive"/>, making it absolute
    /// </summary>
    public static AbsoluteFilePath operator +(DriveLetter drive, RelativeFilePath filePath)
        => new(Path.Combine($"{drive.Value}:", filePath.Value));
    /// <returns>a <see cref="RelativeFilePath"/> of <paramref name="drive"/>://<paramref name="fileName"/></returns>
    public static AbsoluteFilePath operator +(DriveLetter drive, FileNameWithExtension fileName)
        => new(Path.Combine($"{drive.Value}:", fileName.Value));
    /// <returns><paramref name="drive"/> and <paramref name="dir"/> combined into a <see cref="RelativeDirectoryPath"/></returns>
    public static AbsoluteDirectoryPath operator +(DriveLetter drive, DirectoryName dir)
        => new(Path.Combine($"{drive.Value}:", dir.Value));
}