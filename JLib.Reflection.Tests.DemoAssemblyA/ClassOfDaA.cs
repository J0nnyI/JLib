using JLib.Reflection.Tests.DemoAssembly1A;

namespace JLib.Reflection.Tests.DemoAssemblyA;
public class TestAssemblyADemoClassA { }
public class TestAssemblyADemoClassB { }

public class TestAssemblyADemoClassC
{
    private TestAssembly1ADemoClassC? Demo { get; } = null;
}
