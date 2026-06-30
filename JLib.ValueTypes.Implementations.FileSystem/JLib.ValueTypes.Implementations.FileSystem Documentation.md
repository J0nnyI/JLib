# JLib.ValueTypes.Implementations.FileSystem

Provides validated JLib ValueType implementations for file system path segments such as file names, file extensions, directory names, drive letters, and absolute or relative file and directory paths. Built on [JLib.ValueTypes](../JLib.ValueTypes/JLib.ValueTypes%20Documentation.md) to enforce path validity through strongly typed values.

> Note: this package is still under development and its API may change.

## Installation
```sh
dotnet add package JLib.ValueTypes.Implementations.FileSystem
```

## Features
- Strongly typed, validated path segments: `FileNameWithExtension`, `FileNameWithoutExtension`, `FileExtension`, `DirectoryName`, and `DriveLetter`.
- Validated path types with an abstract base and concrete relative/absolute variants:
  - `FilePath` → `AbsoluteFilePath`, `RelativeFilePath`
  - `DirectoryPath` → `AbsoluteDirectoryPath`, `RelativeDirectoryPath`
- Validation rejects invalid values at construction (throws an `AggregateException`), using `System.IO.Path` rules (`GetInvalidPathChars`, `GetInvalidFileNameChars`, `IsPathRooted`, `HasExtension`), so results may differ between operating systems.
- Operator overloads for composing paths (`DirectoryPath + RelativeDirectoryPath`, `DirectoryPath + FileNameWithExtension`, `DriveLetter + RelativeDirectoryPath`, `FileNameWithoutExtension + FileExtension`) and for computing relative paths (`DirectoryPath - AbsoluteDirectoryPath`).
- Convenience members that wrap `System.IO.File` and `System.IO.Directory` directly on the path types (read/write/append, stream operations, copy/move/rename/delete, attribute and timestamp getters/setters, existence checks, directory enumeration).
- A marker interface `IPathSegment` identifying value types that represent a part of a file path.
- Reusable validation extension methods (`FilePathValidationExtensions`) such as `BeRootPath`, `BeRelativePath`, `NotContainInvalidPathChars`, `HaveAnExtension`, and `HaveExtension`.

## Usage

### Creating validated paths
Construction validates the value and throws when it is invalid.
```cs
using JLib.ValueTypes.Implementations.FileSystem;

// valid
var absolute = new AbsoluteFilePath(@"G:\directory\file.extension");
var relative = new RelativeFilePath(@"file/file.extension");

// invalid - a relative path is not rooted, so this throws an AggregateException
var invalid = new AbsoluteFilePath(@"file/file.extension");
```

### Picking relative vs. absolute automatically
`FilePath` and `DirectoryPath` expose `CreateInstance`, which inspects `Path.IsPathRooted` and returns the matching concrete type.
```cs
FilePath path = FilePath.CreateInstance(@"C:\logs\app.log");      // -> AbsoluteFilePath
DirectoryPath dir = DirectoryPath.CreateInstance(@"logs\today");   // -> RelativeDirectoryPath
```

### Composing paths with operators
```cs
DriveLetter drive = new('C');
RelativeDirectoryPath sub = new(@"projects\jlib");

AbsoluteDirectoryPath dir = drive + sub;                 // C:\projects\jlib
AbsoluteFilePath file = dir + new FileNameWithExtension("readme.md");

// build a file name from its parts
FileNameWithExtension name = new FileNameWithoutExtension("report") + new FileExtension("csv"); // report.csv

// compute a relative path: @"a\b\x\y" - @"a\b" => @"x\y"
RelativeDirectoryPath rel = new AbsoluteDirectoryPath(@"C:\a\b\x\y")
                          - new AbsoluteDirectoryPath(@"C:\a\b");
```

### Navigating and inspecting paths
```cs
var dirPath = new RelativeDirectoryPath(@"directory\file.extension");
RelativeDirectoryPath? parent = dirPath.GetParent();      // "directory"

var filePath = new AbsoluteFilePath(@"C:\data\report.csv");
FileNameWithExtension fileName = filePath.GetFileName();  // report.csv
FileExtension ext = fileName.GetExtension();              // csv
FileNameWithoutExtension stem = fileName.RemoveExtension();// report
```

### Working with files and directories
The path types wrap `System.IO.File`/`Directory` so you can operate directly on the typed value.
```cs
var file = new AbsoluteFilePath(@"C:\data\report.csv");

if (!file.Exists())
{
    file.CreateDirectory();
    await file.WriteAllTextAsync("id;name");
}

string content = file.ReadAllText();

var directory = new AbsoluteDirectoryPath(@"C:\data");
foreach (AbsoluteFilePath f in directory.GetFiles())
    Console.WriteLine(f.Value);
```

### Custom validation with the extension methods
`FilePathValidationExtensions` provides reusable checks for building your own value types or validations.
```cs
[Validation]
private static void Validate(ValidationContext<string> must)
    => must
        .BeRootPath()
        .NotContainInvalidPathChars()
        .HaveAnExtension();
```

## Related Packages
- [JLib.ValueTypes](../JLib.ValueTypes/JLib.ValueTypes%20Documentation.md) - the underlying strongly typed value type framework this package builds on.
- [JLib](../JLib/JLib%20Documentation.md) - shared helpers used internally (e.g. `ToReadOnlyCollection`).
