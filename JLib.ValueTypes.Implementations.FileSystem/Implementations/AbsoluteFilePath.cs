namespace JLib.ValueTypes.Implementations.FileSystem;

/// <summary>
/// an absolute path to a file which may or may not exist
/// </summary>
public record AbsoluteFilePath(string Value) : FilePath(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .BeRootPath()
            .NotContainInvalidPathChars()
            .HaveAnDirectory();

    public override DirectoryPath GetDirectory()
        => new AbsoluteDirectoryPath(Path.GetDirectoryName(Value) ?? @"\");
}