namespace JLib.ValueTypes.Implementations.FileSystem;

public abstract record FileSystemPath(string Value) : StringValueType(Value), IPath
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .NotContainInvalidPathChars();
}