using System.Runtime.CompilerServices;

namespace JLib.ValueTypes.Implementations.FileSystem;

/// <summary>
/// either a <see cref="AbsoluteDirectoryPath"/> or <see cref="RelativeDirectoryPath"/>
/// </summary>
/// <param name="Value"></param>
public abstract record DirectoryPath(string Value) : FileSystemPath(Value)
{
    [Validation]
    // ReSharper disable once UnusedMember.Local
    private static void Validate(ValidationContext<string> context)
    {
        context.NotContain(Path.GetInvalidPathChars());
    }

    public static FilePath operator +(DirectoryPath dir, FileNameWithExtension fileName)
        => dir switch
        {
            AbsoluteDirectoryPath abs => abs + fileName,
            RelativeDirectoryPath rel => rel + fileName,
            _ => throw new ArgumentOutOfRangeException()
        };
}