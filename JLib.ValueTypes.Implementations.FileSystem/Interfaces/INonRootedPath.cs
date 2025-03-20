namespace JLib.ValueTypes.Implementations.FileSystem;

/// <summary>
/// every kind of "relative" path.<br/>
/// <example>
/// <b>Windows</b><br/>
/// Correct:<br/>
/// - ".\directory\file.extension"<br/>
/// - "..\directory\file.extension"<br/>
/// - "directory\file.extension"<br/>
/// Incorrect:<br/>
/// - "C:\directory\file.extension"<br/>
/// - "\directory\file.extension"
/// </example>
/// </summary>
public interface INonRootedPath : IPath;