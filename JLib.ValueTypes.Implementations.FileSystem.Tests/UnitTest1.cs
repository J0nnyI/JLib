using FluentAssertions;
using Xunit;

namespace JLib.ValueTypes.Implementations.FileSystem.Tests;

public class UnitTest1
{
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
}