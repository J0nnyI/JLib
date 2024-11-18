using JLib.TypeSystem.Abstractions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace JLib.SourceCodeGenerator.Tests;


public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var @namespace = new TypeSystemValues.Namespace("JLib.SourceCodeGenerator.Tests");
        var genClass = new GeneratedClass("DemoClass")
        {
            Namespace = @namespace,
            Members =
            {
                new GeneratedProperty("",)
            }
        };


    }
}