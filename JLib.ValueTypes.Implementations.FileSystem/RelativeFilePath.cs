namespace JLib.ValueTypes.Implementations.FileSystem;

/// <summary>
/// a relative path to a file which may or may not exist
/// </summary>
public record RelativeFilePath(string Value) : FilePath(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .BeRelativePath()
            .NotContainInvalidPathChars();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dirName"></param>
    /// <returns></returns>
    public static implicit operator RelativeFilePath(FileNameWithExtension dirName)
        => new(dirName);

    public override DirectoryPath GetDirectory() 
        => new RelativeDirectoryPath(Path.GetDirectoryName(Value) ?? "./");
}