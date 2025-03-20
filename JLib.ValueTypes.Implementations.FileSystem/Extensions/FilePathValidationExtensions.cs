using IoPath = System.IO.Path;

namespace JLib.ValueTypes.Implementations.FileSystem;

/// <summary>
/// extension methods for validating file paths
/// </summary>
public static class FilePathValidationExtensions
{
    /// <summary>
    /// checks for <see cref="IoPath.IsPathRooted(ReadOnlySpan{char})"/> = true
    /// </summary>
    public static IValidationContext<string?> BeRootPath(this IValidationContext<string?> context)
    {
        if (IoPath.IsPathRooted(context.Value) is false)
            context.AddError("Must be rooted");
        return context;
    }
    /// <summary>
    /// checks for <see cref="IoPath.IsPathRooted(ReadOnlySpan{char})"/> = false
    /// </summary>
    public static IValidationContext<string?> BeRelativePath(this IValidationContext<string?> context)
    {
        if (IoPath.IsPathRooted(context.Value))
            context.AddError("Must not be rooted");
        return context;
    }

    private static readonly string InvalidPathChars = string.Join(", ", IoPath.GetInvalidPathChars());
    /// <summary>
    /// checks whether the value contains <see cref="IoPath.GetInvalidPathChars"/>
    /// </summary>
    public static IValidationContext<string?> NotContainInvalidPathChars(this IValidationContext<string?> context)
        => context.SatisfyCondition(x => IoPath.GetInvalidPathChars().Contains(x) is false,
            $"Must not contain invalid path chars ({InvalidPathChars})");

    private static readonly string InvalidFileNameChars = string.Join(", ", IoPath.GetInvalidFileNameChars());
    /// <summary>
    /// checks whether the value contains <see cref="IoPath.GetInvalidFileNameChars"/>
    /// </summary>
    public static IValidationContext<string?> NotContainInvalidFileNameChars(this IValidationContext<string?> context)
        => context.SatisfyCondition(x => IoPath.GetInvalidFileNameChars().Contains(x) is false,
            $"Must not contain invalid path chars ({InvalidFileNameChars})");

    /// <summary>
    /// checks for <see cref="IoPath.HasExtension(ReadOnlySpan{char})"/> = true
    /// </summary>
    public static IValidationContext<string?> HaveAnExtension(this IValidationContext<string?> context)
    {
        if (IoPath.HasExtension(context.Value) is false)
            context.AddError("Must have an extension");
        return context;
    }
    /// <summary>
    /// checks for <see cref="IoPath.GetExtension(ReadOnlySpan{char})"/> = <paramref name="extension"/>
    /// </summary>
    public static IValidationContext<string?> HaveExtension(this IValidationContext<string?> context, FileExtension extension)
    {
        if (IoPath.GetExtension(context.Value) != extension.Value)
            context.AddError($"Must have the '{extension.Value}' extension but has '{extension.Value}'");
        return context;
    }
    /// <summary>
    /// checks for <see cref="IoPath.HasExtension(ReadOnlySpan{char})"/> = false
    /// </summary>
    public static IValidationContext<string?> HaveNoExtension(this IValidationContext<string?> context)
    {
        if (IoPath.HasExtension(context.Value))
            context.AddError("Must have no extension");
        return context;
    }
    public static IValidationContext<string?> HaveAnDirectory(this IValidationContext<string?> context)
    {
        if (IoPath.GetDirectoryName(context.Value) is null)
            context.AddError($"Must have an directory");
        return context;
    }
}