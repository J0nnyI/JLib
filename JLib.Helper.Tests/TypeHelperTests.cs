using FluentAssertions;
using Xunit;

namespace JLib.Helper.Tests;

public class TypeHelperTests
{
    public class SubA
    {
        public class SubAa
        {

        }
    }
    [Fact]
    public void GetNestingParentsTest()
    {
        typeof(SubA.SubAa).GetNestingParents().Should().ContainInOrder(new[]
        {
            typeof(TypeHelperTests), typeof(SubA), typeof(SubA.SubAa)
        });
    }
}