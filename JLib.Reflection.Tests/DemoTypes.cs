namespace JLib.Reflection.Tests;

public static class DemoTypes
{
    #region nested classes
    public class NestingDemoClass
    {
        public class NestedDemoClassA { }
        public class NestedDemoClassB { }
        public class NestedDemoClassC { }
    }
    public class NestingDemoClass2
    {
        public class NestedDemoClass2A
        {
            public class NestedDemoClass2A1{}
        }
        public class NestedDemoClass2B { }
        public class NestedDemoClass2C { }
    }
    public static IReadOnlyCollection<Type> NestedTypes =>
    [
        typeof(NestingDemoClass.NestedDemoClassA),
        typeof(NestingDemoClass.NestedDemoClassB),
        typeof(NestingDemoClass.NestedDemoClassC)
    ];
    public static IReadOnlyCollection<Type> NestedTypes2 =>
    [
        typeof(NestingDemoClass2.NestedDemoClass2A),
        typeof(NestingDemoClass2.NestedDemoClass2A.NestedDemoClass2A1),
        typeof(NestingDemoClass2.NestedDemoClass2B),
        typeof(NestingDemoClass2.NestedDemoClass2C)
    ];
    #endregion
    #region direct classes
    public class DemoClassA { }
    public class DemoClassB { }
    public class DemoClassC { }
    public static readonly IReadOnlyCollection<Type> Types =
    [
        typeof(DemoClassA),
        typeof(DemoClassB),
        typeof(DemoClassC)
    ];
    #endregion

}