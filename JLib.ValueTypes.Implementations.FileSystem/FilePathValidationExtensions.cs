namespace JLib.ValueTypes.Implementations.FileSystem;

/// <summary>
/// extension methods for validating file paths
/// </summary>
public static class FilePathValidationExtensions
{
    /// <summary>
    /// checks for <see cref="Path.IsPathRooted(ReadOnlySpan{char})"/> = true
    /// </summary>
    public static IValidationContext<string?> BeRootPath(this IValidationContext<string?> context)
    {
        if (Path.IsPathRooted(context.Value) is false)
            context.Fail("Must be rooted");
        return context;
    }
    /// <summary>
    /// checks for <see cref="Path.IsPathRooted(ReadOnlySpan{char})"/> = false
    /// </summary>
    public static IValidationContext<string?> BeRelativePath(this IValidationContext<string?> context)
    {
        if (Path.IsPathRooted(context.Value))
            context.Fail("Must not be rooted");
        return context;
    }

    private static readonly string InvalidPathChars = string.Join(", ", Path.GetInvalidPathChars());
    /// <summary>
    /// checks whether the value contains <see cref="Path.GetInvalidPathChars"/>
    /// </summary>
    public static IValidationContext<string?> NotContainInvalidPathChars(this IValidationContext<string?> context)
        => context.SatisfyCondition(x => Path.GetInvalidPathChars().Contains(x) is false,
            $"Must not contain invalid path chars ({InvalidPathChars})");

    private static readonly string InvalidFileNameChars = string.Join(", ", Path.GetInvalidFileNameChars());
    /// <summary>
    /// checks whether the value contains <see cref="Path.GetInvalidFileNameChars"/>
    /// </summary>
    public static IValidationContext<string?> NotContainInvalidFileNameChars(this IValidationContext<string?> context)
        => context.SatisfyCondition(x => Path.GetInvalidFileNameChars().Contains(x) is false,
            $"Must not contain invalid path chars ({InvalidFileNameChars})");

    /// <summary>
    /// checks for <see cref="Path.HasExtension(ReadOnlySpan{char})"/> = true
    /// </summary>
    public static IValidationContext<string?> HaveAnExtension(this IValidationContext<string?> context)
    {
        if (Path.HasExtension(context.Value) is false)
            context.Fail("Must have an extension");
        return context;
    }
    /// <summary>
    /// checks for <see cref="Path.GetExtension(ReadOnlySpan{char})"/> = <paramref name="extension"/>
    /// </summary>
    public static IValidationContext<string?> HaveExtension(this IValidationContext<string?> context, FileExtension extension)
    {
        if (Path.GetExtension(context.Value) != extension.Value)
            context.Fail($"Must have the '{extension.Value}' extension but has '{extension.Value}'");
        return context;
    }
    /// <summary>
    /// checks for <see cref="Path.HasExtension(ReadOnlySpan{char})"/> = false
    /// </summary>
    public static IValidationContext<string?> HaveNoExtension(this IValidationContext<string?> context)
    {
        if (Path.HasExtension(context.Value))
            context.Fail("Must have no extension");
        return context;
    }
    public static IValidationContext<string?> HaveAnDirectory(this IValidationContext<string?> context)
    {
        if (Path.GetDirectoryName(context.Value) is null)
            context.Fail($"Must have an directory");
        return context;
    }
}