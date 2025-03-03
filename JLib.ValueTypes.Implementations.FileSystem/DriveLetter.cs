namespace JLib.ValueTypes.Implementations.FileSystem;

/// <summary>
/// The letter of a windows filesystem drive, e.g. 'C'
/// </summary>
public record DriveLetter(char Value) : CharValueType(Value)
{
    [Validation]
    private static void Validate(ValidationContext<char> must)
        => must.BeAsciiLetter();

    /// <summary>
    /// appends the <paramref name="path"/> to the <paramref name="letter"/>, making it absolute
    /// </summary>
    public static AbsoluteDirectoryPath operator +(DriveLetter letter, RelativeDirectoryPath path)
        => new($"{letter.Value}:{Path.DirectorySeparatorChar}{path.Value}");
    /// <summary>
    /// appends the <paramref name="filePath"/> to the <paramref name="letter"/>, making it absolute
    /// </summary>
    public static AbsoluteDirectoryPath operator +(DriveLetter letter, RelativeFilePath filePath)
        => new($"{letter.Value}:{Path.DirectorySeparatorChar}{filePath.Value}");
}