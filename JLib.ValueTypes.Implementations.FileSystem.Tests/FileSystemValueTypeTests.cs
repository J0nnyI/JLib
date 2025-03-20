using FluentAssertions;

using Xunit;

namespace JLib.ValueTypes.Implementations.FileSystem.Tests;

public class FileSystemValueTypeTests
{
    public class PathTests
    {
        [Fact]
        public void AbsoluteDirectoryPath_Valid()
        {
            // this should not throw
            new AbsoluteDirectoryPath(@"G:\directory\dir");
        }
        [Fact]
        public void AbsoluteDirectoryPath_Valid2()
        {
            // this should not throw
            new AbsoluteDirectoryPath(@"\directory\dir");
        }
        [Fact]
        public void AbsoluteDirectoryPath_Invalid()
        {
            // this should not throw
            var act = () => new AbsoluteDirectoryPath(
                @"directory/dir");
            act.Should().Throw<AggregateException>();
        }
        [Fact]
        public void RelativeDirectoryPath_Valid()
        {
            // this should not throw
            new RelativeDirectoryPath(@".\directory\dir");
        }
        [Fact]
        public void RelativeDirectoryPath_Valid2()
        {
            // this should not throw
            new RelativeDirectoryPath(@"directory\dir");
        }
        [Fact]
        public void RelativeDirectoryPath_Invalid()
        {
            // this should not throw
            var act = () => new RelativeDirectoryPath(@"G:\directory\dir");
            act.Should().Throw<AggregateException>();
        }
        [Fact]
        public void AbsoluteFilePath_Valid()
        {
            // this should not throw
            new AbsoluteFilePath(
                @"G:\directory\file.extension");
        }
        [Fact]
        public void AbsoluteFilePath_Invalid()
        {
            // this should not throw
            var act = () => new AbsoluteFilePath(
                @"file/file.extension");
            act.Should().Throw<AggregateException>();
        }


        [Fact]
        public void RelativeFilePath_Valid()
        {
            // this should not throw
            new RelativeFilePath(
                @"file/file.extension");
        }
        [Fact]
        public void RelativeFilePath_Invalid()
        {
            // this should not throw
            var act = () => new RelativeFilePath(
                @"G:\directory\file.extension");
            act.Should().Throw<AggregateException>();
        }
    }

    public class NameTests
    {

        [Fact]
        public void DirectoryName_Valid()
        {
            // this should not throw
            new FileNameWithoutExtension(
                @"file");
        }
        [Fact]
        public void DirectoryName_Invalid()
        {
            // this should not throw
            var act = () => new FileNameWithoutExtension(
                """file" """);
            act.Should().Throw<AggregateException>();
        }
        [Fact]
        public void FileNameWithoutExtension_Valid()
        {
            // this should not throw
            new FileNameWithoutExtension(
                @"file");
        }
        [Fact]
        public void FileNameWithoutExtension_Invalid()
        {
            // this should not throw
            var act = () => new FileNameWithoutExtension(
                @"file.ext");
            act.Should().Throw<AggregateException>();
        }


        [Fact]
        public void FileNameWithExtension_Valid()
        {
            // this should not throw
            new FileNameWithExtension(
                @"file.ext");
        }
        [Fact]
        public void FileNameWithExtension_Invalid()
        {
            // this should not throw
            var act = () => new FileNameWithExtension(
                @"file");
            act.Should().Throw<AggregateException>();
        }
        [Fact]
        public void FileNameWithExtension_Invalid2()
        {
            // this should not throw
            var act = () => new FileNameWithExtension(
                @"dir/file.ext");
            act.Should().Throw<AggregateException>();
        }
    }
    public class OtherTests
    {
        [Fact]
        public void DriveLetter_Valid()
        {
            // this should not throw
            new DriveLetter('C');
        }
        [Fact]
        public void DriveLetter_Invalid()
        {
            // this should not throw
            var act = () => new DriveLetter('#');
            act.Should().Throw<AggregateException>();
        }
        [Fact]
        public void FileExtension_Valid()
        {
            // this should not throw
            new FileExtension("txt");
        }
        [Fact]
        public void FileExtension_Invalid()
        {
            var act = () => new FileExtension(".txt");
            act.Should().Throw<AggregateException>();
        }
        [Fact]
        public void FileExtension_Invalid2()
        {
            var act = () => new FileExtension("/txt");
            act.Should().Throw<AggregateException>();
        }
    }

    public class OperatorTests
    {
        [Fact]
        public void FileName_FileExt()
        {
            (new FileNameWithoutExtension("file") + new FileExtension("ext")).Should()
                .Be(new FileNameWithExtension("file.ext"));
        }
        [Fact]
        public void DirName_FileName()
        {
            (new DirectoryName("directory") + new FileNameWithExtension("file.ext")).Should()
                .Be(new RelativeFilePath(@"directory\file.ext"));
        }
        [Fact]
        public void DirName_DirName()
        {
            (new DirectoryName("directory1") + new DirectoryName("directory2")).Should()
                .Be(new RelativeDirectoryPath(@"directory1\directory2"));
        }


        [Fact]
        public void RelDirPath_FileName()
        {
            (new RelativeDirectoryPath("dir/dir2") + new FileNameWithExtension("file.ext")).Should()
                .Be(new RelativeFilePath(@"dir/dir2\file.ext"));
        }
        [Fact]
        public void AbsDirPath_FileName()
        {
            (new AbsoluteDirectoryPath(@"C:/dir/dir2") + new FileNameWithExtension("file.ext")).Should()
                .Be(new AbsoluteFilePath(@"C:/dir/dir2\file.ext"));
        }

        [Fact]
        public void RelDirPath_FilePath()
        {
            (new RelativeDirectoryPath("dir/dir2") + new RelativeFilePath("fileDir/file.ext")).Should()
                .Be(new RelativeFilePath(@"dir/dir2\fileDir/file.ext"));
        }
        [Fact]
        public void AbsDirPath_FilePath()
        {
            (new AbsoluteDirectoryPath("C:/dir/dir2") + new RelativeFilePath("fileDir/file.ext")).Should()
                .Be(new AbsoluteFilePath(@"C:/dir/dir2\fileDir/file.ext"));
        }

        [Fact]
        public void DriveLetter_FileName()
        {
            (new DriveLetter('C') + new FileNameWithExtension("file.ext")).Should()
                .Be(new AbsoluteFilePath(@"C:\file.ext"));
        }
        [Fact]
        public void DriveLetter_DirName()
        {
            var res = (new DriveLetter('C') + new DirectoryName("directory"));
            var comp = new AbsoluteDirectoryPath(@"C:\directory");
            res.Should().Be(comp);
        }
        [Fact]
        public void DriveLetter_FilePath()
        {
            (new DriveLetter('C') + new RelativeFilePath("dir/file.ext")).Should()
                .Be(new AbsoluteFilePath(@"C:\dir/file.ext"));
        }
        [Fact]
        public void DriveLetter_DirPath()
        {
            var res = (new DriveLetter('C') + new RelativeDirectoryPath("directory/directory2"));
            var comp = new AbsoluteDirectoryPath(@"C:\directory/directory2");
            res.Should().Be(comp);
        }
    }
}