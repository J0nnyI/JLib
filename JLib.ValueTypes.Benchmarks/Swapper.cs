using System.Reflection;
using BenchmarkDotNet.Attributes;
using HarmonyLib;
using JLib.Exceptions;
using Xunit;

namespace JLib.ValueTypes.Benchmarks;

[Trait("TestType", "Benchmark")]
public partial class PerformanceTest
{
    public enum PatchStateEnum
    {
        Patch,
        LockedPatch,
        Unpatched
    }

    public class Swappers
    {
        // Create a Harmony instance
        private static readonly Harmony Harmony = new Harmony("com.example.patch");

        // Get the method to patch
        private static readonly MethodInfo OriginalMethod = typeof(ExceptionBuilder)!.GetMethod("GetException")!;

        [Params(PatchStateEnum.Patch, PatchStateEnum.LockedPatch, PatchStateEnum.Unpatched)]
        // setter required by benchmark.net
        public PatchStateEnum PatchState { get; set; }

        [Benchmark]
        public string FiveCharacterString()
            => FiveCharacterStringCreate(GetRandom5LetterString).Value;

        [GlobalSetup(Target = nameof(FiveCharacterString))]
        public void GlobalSetup2()
        {
            Harmony.Unpatch(OriginalMethod, HarmonyPatchType.All, "com.example.patch");
            switch (PatchState)
            {
                case PerformanceTest.PatchStateEnum.Patch:
                    Harmony.Patch(OriginalMethod, new HarmonyMethod(typeof(PatchClass).GetMethod(nameof(PatchClass.Prefix))!));
                    break;
                case PerformanceTest.PatchStateEnum.LockedPatch:
                    Harmony.Patch(OriginalMethod, new HarmonyMethod(typeof(PatchClass).GetMethod(nameof(PatchClass.LockedPrefix))!));
                    break;
                case PatchStateEnum.Unpatched:
                default:
                    break;
            }
        }

        // #region Experimental Patching
        public class PatchClass
        {
            public static bool LockedPrefix(List<Exception> exceptions, List<IExceptionProvider> children, object exceptionLock, object childrenLock)
            {
                lock (exceptionLock)
                    lock (childrenLock)
                        return Prefix(exceptions, children);
            }

            public static bool Prefix(List<Exception> exceptions, List<IExceptionProvider> children)
            {
                bool condition = exceptions.Count == 0 && children.Count == 0;
                return condition is false;
                // Returning false skips the original method
                // Returning true allows the original method to execute
            }
        }
    }
}