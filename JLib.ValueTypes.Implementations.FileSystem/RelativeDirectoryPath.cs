namespace JLib.ValueTypes.Implementations.FileSystem;

/// <summary>
/// a relative directory path <see cref="AbsoluteDirectoryPath"/>. It may contain only one directory and may use relative navigation<br/><br/>
/// Validation may differ between operating systems due to the usage of <see cref="Path.GetInvalidPathChars"/>
/// <remarks>
/// <br/><br/>
/// <see cref="Path.IsPathRooted(ReadOnlySpan{char})"/> must evaluate <paramref name="Value"/> to false
/// must not contain <see cref="Path.GetInvalidPathChars"/><br/>
/// may contain <see cref="Path.DirectorySeparatorChar"/><br/>
/// may contain <see cref="Path.AltDirectorySeparatorChar"/><br/>
/// </remarks>
/// </summary>
/// <param name="Value">the directory name</param>
public record RelativeDirectoryPath(string Value) : StringValueType(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string> context)
    {
        if (Path.IsPathRooted(context.Value))
            context.Fail("The path must not be rooted");
        context.NotContain(Path.GetInvalidPathChars());
    }
    /// <returns>the directory which contains this directory</returns>
    public RelativeDirectoryPath? GetParent()
        => ValueType.CreateNullable<RelativeDirectoryPath, string>(Path.GetDirectoryName(Value));
    /// <returns>the name of the current directory</returns>
    public DirectoryName GetCurrent()
        => new(Path.GetFileName(Value));
}