 using JLib.Helper;

namespace JLib.ValueTypes.Implementations.FileSystem;

/// <summary>
/// a rooted path to a directory which may or may not exist
/// </summary>
public record AbsoluteDirectoryPath(string Value) : DirectoryPath(Value)
{
    [Validation]
    // ReSharper disable once UnusedMember.Local
    private static void Validate(ValidationContext<string> must)
        => must
            .BeRootPath();

    /// <summary>
    /// appends the <paramref name="relativePath"/> to the <param name="path"/> and returns the resulting <see cref="AbsoluteDirectoryPath"/>
    /// </summary>
    public static AbsoluteDirectoryPath operator +(AbsoluteDirectoryPath path, RelativeDirectoryPath relativePath)
        => new(Path.Combine(path.Value, relativePath.Value));

    /// <summary>
    /// returns
    /// <remarks>
    /// <example>
    /// <code>@"a\b\x\y" - @"a\b" => @"x\y"</code>
    /// </example>
    /// </remarks>
    /// </summary>
    public static RelativeDirectoryPath operator -(AbsoluteDirectoryPath path, AbsoluteDirectoryPath absolutePathTobeRemoved)
        => new(Path.GetRelativePath(absolutePathTobeRemoved.Value, path.Value));


    /// <summary>
    /// appends the <paramref name="file"/> to the <param name="path"/> and returns the resulting <see cref="AbsoluteFilePath"/>
    /// </summary>
    public static AbsoluteFilePath operator +(AbsoluteDirectoryPath path, FileNameWithExtension file)
        => new(Path.Combine(path.Value, file.Value));

    /// <summary>
    /// appends the <paramref name="filePath"/> to the <param name="path"/> and returns the resulting <see cref="AbsoluteFilePath"/>
    /// </summary>
    public static AbsoluteFilePath operator +(AbsoluteDirectoryPath path, RelativeFilePath filePath)
        => new(Path.Combine(path.Value, filePath.Value));

    /// <returns>All files contained in this directory</returns>
    public IReadOnlyCollection<AbsoluteFilePath> GetFiles()
        => Directory.GetFiles(Value).Select(x => new AbsoluteFilePath(x)).ToReadOnlyCollection();

    /// <returns>All Subdirectories of this directory</returns>
    public IReadOnlyCollection<AbsoluteDirectoryPath> GetDirectories()
        => Directory.GetDirectories(Value).Select(x => new AbsoluteDirectoryPath(x)).ToReadOnlyCollection();

    /// <returns>The directory which contains this directory</returns>
    public AbsoluteDirectoryPath? GetParent()
        => ValueType.CreateNullable<AbsoluteDirectoryPath, string>(Path.GetDirectoryName(Value));
    /// <returns>The name of the current directory</returns>
    public DirectoryName GetCurrent()
        => new(Path.GetFileName(Value));
    /// <returns>whether this directory exists or not</returns>
    public bool Exists()
        => Directory.Exists(Value);
    /// <summary>
    /// Creates this directory
    /// </summary>
    public void Create() => Directory.CreateDirectory(Value);

}