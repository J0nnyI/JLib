using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

using Xunit;

namespace JLib.Helper.Tests;
public class StringHelperTests
{
    [Fact]
    public void RemoveLinesWhere1()
    {
        "1,2,3".RemoveSubstringsWhere(x => x == "2", ",")
            .Should().Be("1,3");
        "1,2,3".RemoveSubstringsWhere(x => x == "1", ",")
            .Should().Be("2,3");
        "1,2,3".RemoveSubstringsWhere(x => x == "3", ",")
            .Should().Be("1,2");
    }
    [Fact]
    public void RemoveLinesWhere3()
    {
        "1,2,3".RemoveSubstringsWhere((p, c, n) => p == "1", ",")
            .Should().Be("1,3");
        "1,2,3".RemoveSubstringsWhere((p, c, n) => c == "1", ",")
            .Should().Be("2,3");
        "1,2,3".RemoveSubstringsWhere((p, c, n) => n == "1", ",")
            .Should().Be("1,2,3");

        "1,2,3".RemoveSubstringsWhere((p, c, n) => p == "2", ",")
            .Should().Be("1,2");
        "1,2,3".RemoveSubstringsWhere((p, c, n) => c == "2", ",")
            .Should().Be("1,3");
        "1,2,3".RemoveSubstringsWhere((p, c, n) => n == "2", ",")
            .Should().Be("2,3");

        "1,2,3".RemoveSubstringsWhere((p, c, n) => p == "3", ",")
            .Should().Be("1,2,3");
        "1,2,3".RemoveSubstringsWhere((p, c, n) => c == "3", ",")
            .Should().Be("1,2");
        "1,2,3".RemoveSubstringsWhere((p, c, n) => n == "3", ",")
            .Should().Be("1,3");
    }
}
