using JLib.Exceptions;
using JLib.Helper;

namespace JLib.DataGeneration;

/// <summary>
/// a base class for a <see cref="DataPackage"/> which does not define new data but bundles other <see cref="DataPackage"/>s
/// </summary>
public abstract class DataPackageCollection : DataPackage
{
    /// <summary>
    /// <inheritdoc cref="DataPackageCollection"/>
    /// </summary>
    protected DataPackageCollection(IServiceProvider serviceProvider, params Type[] includedPackages) : base(serviceProvider)
    {
        includedPackages
            .Where(x => x.IsAssignableTo<DataPackage>() is false)
            .Select(x => new DataPackageException.InitializationException.NotADataPackageException(x))
            .ThrowExceptionIfNotEmpty($"Some given {includedPackages} are not derived from {typeof(DataPackage).FullName}");

        IncludeDataPackages(includedPackages);
    }
}

#region generated, generic data package collections
// generated using this code
//var sb = new StringBuilder();
//for (int i = 1; i <= 20; i++)
//{
//    sb.AppendLine("    /// <summary>")
//        .AppendLine("""    /// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>""")
//        .AppendLine("    /// </summary>")
//        .Append("    public sealed class DataPackageCollection<")
//        .AppendJoin(", ", Enumerable.Range(1, i).Select(i => $"TDp{i}"))
//        .AppendLine(">(IServiceProvider provider)")
//        .Append(": DataPackageCollection(provider, ")
//        .AppendJoin(", ", Enumerable.Range(1, i).Select(i => $"typeof(TDp{i})"))
//        .AppendLine(")")
//        .AppendJoin(Environment.NewLine, Enumerable.Range(1, i).Select(i => $"        where TDp{i} : DataPackage")).AppendLine()
//        .AppendLine($";");
//}
//Console.WriteLine(sb)

/// <summary>
/// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>
/// </summary>
public sealed class DataPackageCollection<TDp1>(IServiceProvider provider)
: DataPackageCollection(provider, typeof(TDp1))
    where TDp1 : DataPackage
;
/// <summary>
/// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>
/// </summary>
public sealed class DataPackageCollection<TDp1, TDp2>(IServiceProvider provider)
: DataPackageCollection(provider, typeof(TDp1), typeof(TDp2))
    where TDp1 : DataPackage
    where TDp2 : DataPackage
;
/// <summary>
/// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>
/// </summary>
public sealed class DataPackageCollection<TDp1, TDp2, TDp3>(IServiceProvider provider)
: DataPackageCollection(provider, typeof(TDp1), typeof(TDp2), typeof(TDp3))
    where TDp1 : DataPackage
    where TDp2 : DataPackage
    where TDp3 : DataPackage
;
/// <summary>
/// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>
/// </summary>
public sealed class DataPackageCollection<TDp1, TDp2, TDp3, TDp4>(IServiceProvider provider)
: DataPackageCollection(provider, typeof(TDp1), typeof(TDp2), typeof(TDp3), typeof(TDp4))
    where TDp1 : DataPackage
    where TDp2 : DataPackage
    where TDp3 : DataPackage
    where TDp4 : DataPackage
;
/// <summary>
/// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>
/// </summary>
public sealed class DataPackageCollection<TDp1, TDp2, TDp3, TDp4, TDp5>(IServiceProvider provider)
: DataPackageCollection(provider, typeof(TDp1), typeof(TDp2), typeof(TDp3), typeof(TDp4), typeof(TDp5))
    where TDp1 : DataPackage
    where TDp2 : DataPackage
    where TDp3 : DataPackage
    where TDp4 : DataPackage
    where TDp5 : DataPackage
;
/// <summary>
/// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>
/// </summary>
public sealed class DataPackageCollection<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6>(IServiceProvider provider)
: DataPackageCollection(provider, typeof(TDp1), typeof(TDp2), typeof(TDp3), typeof(TDp4), typeof(TDp5), typeof(TDp6))
    where TDp1 : DataPackage
    where TDp2 : DataPackage
    where TDp3 : DataPackage
    where TDp4 : DataPackage
    where TDp5 : DataPackage
    where TDp6 : DataPackage
;
/// <summary>
/// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>
/// </summary>
public sealed class DataPackageCollection<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7>(IServiceProvider provider)
: DataPackageCollection(provider, typeof(TDp1), typeof(TDp2), typeof(TDp3), typeof(TDp4), typeof(TDp5), typeof(TDp6), typeof(TDp7))
    where TDp1 : DataPackage
    where TDp2 : DataPackage
    where TDp3 : DataPackage
    where TDp4 : DataPackage
    where TDp5 : DataPackage
    where TDp6 : DataPackage
    where TDp7 : DataPackage
;
/// <summary>
/// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>
/// </summary>
public sealed class DataPackageCollection<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8>(IServiceProvider provider)
: DataPackageCollection(provider, typeof(TDp1), typeof(TDp2), typeof(TDp3), typeof(TDp4), typeof(TDp5), typeof(TDp6), typeof(TDp7), typeof(TDp8))
    where TDp1 : DataPackage
    where TDp2 : DataPackage
    where TDp3 : DataPackage
    where TDp4 : DataPackage
    where TDp5 : DataPackage
    where TDp6 : DataPackage
    where TDp7 : DataPackage
    where TDp8 : DataPackage
;
/// <summary>
/// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>
/// </summary>
public sealed class DataPackageCollection<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9>(IServiceProvider provider)
: DataPackageCollection(provider, typeof(TDp1), typeof(TDp2), typeof(TDp3), typeof(TDp4), typeof(TDp5), typeof(TDp6), typeof(TDp7), typeof(TDp8), typeof(TDp9))
    where TDp1 : DataPackage
    where TDp2 : DataPackage
    where TDp3 : DataPackage
    where TDp4 : DataPackage
    where TDp5 : DataPackage
    where TDp6 : DataPackage
    where TDp7 : DataPackage
    where TDp8 : DataPackage
    where TDp9 : DataPackage
;
/// <summary>
/// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>
/// </summary>
public sealed class DataPackageCollection<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10>(IServiceProvider provider)
: DataPackageCollection(provider, typeof(TDp1), typeof(TDp2), typeof(TDp3), typeof(TDp4), typeof(TDp5), typeof(TDp6), typeof(TDp7), typeof(TDp8), typeof(TDp9), typeof(TDp10))
    where TDp1 : DataPackage
    where TDp2 : DataPackage
    where TDp3 : DataPackage
    where TDp4 : DataPackage
    where TDp5 : DataPackage
    where TDp6 : DataPackage
    where TDp7 : DataPackage
    where TDp8 : DataPackage
    where TDp9 : DataPackage
    where TDp10 : DataPackage
;
/// <summary>
/// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>
/// </summary>
public sealed class DataPackageCollection<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11>(IServiceProvider provider)
: DataPackageCollection(provider, typeof(TDp1), typeof(TDp2), typeof(TDp3), typeof(TDp4), typeof(TDp5), typeof(TDp6), typeof(TDp7), typeof(TDp8), typeof(TDp9), typeof(TDp10), typeof(TDp11))
    where TDp1 : DataPackage
    where TDp2 : DataPackage
    where TDp3 : DataPackage
    where TDp4 : DataPackage
    where TDp5 : DataPackage
    where TDp6 : DataPackage
    where TDp7 : DataPackage
    where TDp8 : DataPackage
    where TDp9 : DataPackage
    where TDp10 : DataPackage
    where TDp11 : DataPackage
;
/// <summary>
/// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>
/// </summary>
public sealed class DataPackageCollection<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12>(IServiceProvider provider)
: DataPackageCollection(provider, typeof(TDp1), typeof(TDp2), typeof(TDp3), typeof(TDp4), typeof(TDp5), typeof(TDp6), typeof(TDp7), typeof(TDp8), typeof(TDp9), typeof(TDp10), typeof(TDp11), typeof(TDp12))
    where TDp1 : DataPackage
    where TDp2 : DataPackage
    where TDp3 : DataPackage
    where TDp4 : DataPackage
    where TDp5 : DataPackage
    where TDp6 : DataPackage
    where TDp7 : DataPackage
    where TDp8 : DataPackage
    where TDp9 : DataPackage
    where TDp10 : DataPackage
    where TDp11 : DataPackage
    where TDp12 : DataPackage
;
/// <summary>
/// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>
/// </summary>
public sealed class DataPackageCollection<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13>(IServiceProvider provider)
: DataPackageCollection(provider, typeof(TDp1), typeof(TDp2), typeof(TDp3), typeof(TDp4), typeof(TDp5), typeof(TDp6), typeof(TDp7), typeof(TDp8), typeof(TDp9), typeof(TDp10), typeof(TDp11), typeof(TDp12), typeof(TDp13))
    where TDp1 : DataPackage
    where TDp2 : DataPackage
    where TDp3 : DataPackage
    where TDp4 : DataPackage
    where TDp5 : DataPackage
    where TDp6 : DataPackage
    where TDp7 : DataPackage
    where TDp8 : DataPackage
    where TDp9 : DataPackage
    where TDp10 : DataPackage
    where TDp11 : DataPackage
    where TDp12 : DataPackage
    where TDp13 : DataPackage
;
/// <summary>
/// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>
/// </summary>
public sealed class DataPackageCollection<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13, TDp14>(IServiceProvider provider)
: DataPackageCollection(provider, typeof(TDp1), typeof(TDp2), typeof(TDp3), typeof(TDp4), typeof(TDp5), typeof(TDp6), typeof(TDp7), typeof(TDp8), typeof(TDp9), typeof(TDp10), typeof(TDp11), typeof(TDp12), typeof(TDp13), typeof(TDp14))
    where TDp1 : DataPackage
    where TDp2 : DataPackage
    where TDp3 : DataPackage
    where TDp4 : DataPackage
    where TDp5 : DataPackage
    where TDp6 : DataPackage
    where TDp7 : DataPackage
    where TDp8 : DataPackage
    where TDp9 : DataPackage
    where TDp10 : DataPackage
    where TDp11 : DataPackage
    where TDp12 : DataPackage
    where TDp13 : DataPackage
    where TDp14 : DataPackage
;
/// <summary>
/// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>
/// </summary>
public sealed class DataPackageCollection<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13, TDp14, TDp15>(IServiceProvider provider)
: DataPackageCollection(provider, typeof(TDp1), typeof(TDp2), typeof(TDp3), typeof(TDp4), typeof(TDp5), typeof(TDp6), typeof(TDp7), typeof(TDp8), typeof(TDp9), typeof(TDp10), typeof(TDp11), typeof(TDp12), typeof(TDp13), typeof(TDp14), typeof(TDp15))
    where TDp1 : DataPackage
    where TDp2 : DataPackage
    where TDp3 : DataPackage
    where TDp4 : DataPackage
    where TDp5 : DataPackage
    where TDp6 : DataPackage
    where TDp7 : DataPackage
    where TDp8 : DataPackage
    where TDp9 : DataPackage
    where TDp10 : DataPackage
    where TDp11 : DataPackage
    where TDp12 : DataPackage
    where TDp13 : DataPackage
    where TDp14 : DataPackage
    where TDp15 : DataPackage
;
/// <summary>
/// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>
/// </summary>
public sealed class DataPackageCollection<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13, TDp14, TDp15, TDp16>(IServiceProvider provider)
: DataPackageCollection(provider, typeof(TDp1), typeof(TDp2), typeof(TDp3), typeof(TDp4), typeof(TDp5), typeof(TDp6), typeof(TDp7), typeof(TDp8), typeof(TDp9), typeof(TDp10), typeof(TDp11), typeof(TDp12), typeof(TDp13), typeof(TDp14), typeof(TDp15), typeof(TDp16))
    where TDp1 : DataPackage
    where TDp2 : DataPackage
    where TDp3 : DataPackage
    where TDp4 : DataPackage
    where TDp5 : DataPackage
    where TDp6 : DataPackage
    where TDp7 : DataPackage
    where TDp8 : DataPackage
    where TDp9 : DataPackage
    where TDp10 : DataPackage
    where TDp11 : DataPackage
    where TDp12 : DataPackage
    where TDp13 : DataPackage
    where TDp14 : DataPackage
    where TDp15 : DataPackage
    where TDp16 : DataPackage
;
/// <summary>
/// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>
/// </summary>
public sealed class DataPackageCollection<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13, TDp14, TDp15, TDp16, TDp17>(IServiceProvider provider)
: DataPackageCollection(provider, typeof(TDp1), typeof(TDp2), typeof(TDp3), typeof(TDp4), typeof(TDp5), typeof(TDp6), typeof(TDp7), typeof(TDp8), typeof(TDp9), typeof(TDp10), typeof(TDp11), typeof(TDp12), typeof(TDp13), typeof(TDp14), typeof(TDp15), typeof(TDp16), typeof(TDp17))
    where TDp1 : DataPackage
    where TDp2 : DataPackage
    where TDp3 : DataPackage
    where TDp4 : DataPackage
    where TDp5 : DataPackage
    where TDp6 : DataPackage
    where TDp7 : DataPackage
    where TDp8 : DataPackage
    where TDp9 : DataPackage
    where TDp10 : DataPackage
    where TDp11 : DataPackage
    where TDp12 : DataPackage
    where TDp13 : DataPackage
    where TDp14 : DataPackage
    where TDp15 : DataPackage
    where TDp16 : DataPackage
    where TDp17 : DataPackage
;
/// <summary>
/// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>
/// </summary>
public sealed class DataPackageCollection<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13, TDp14, TDp15, TDp16, TDp17, TDp18>(IServiceProvider provider)
: DataPackageCollection(provider, typeof(TDp1), typeof(TDp2), typeof(TDp3), typeof(TDp4), typeof(TDp5), typeof(TDp6), typeof(TDp7), typeof(TDp8), typeof(TDp9), typeof(TDp10), typeof(TDp11), typeof(TDp12), typeof(TDp13), typeof(TDp14), typeof(TDp15), typeof(TDp16), typeof(TDp17), typeof(TDp18))
    where TDp1 : DataPackage
    where TDp2 : DataPackage
    where TDp3 : DataPackage
    where TDp4 : DataPackage
    where TDp5 : DataPackage
    where TDp6 : DataPackage
    where TDp7 : DataPackage
    where TDp8 : DataPackage
    where TDp9 : DataPackage
    where TDp10 : DataPackage
    where TDp11 : DataPackage
    where TDp12 : DataPackage
    where TDp13 : DataPackage
    where TDp14 : DataPackage
    where TDp15 : DataPackage
    where TDp16 : DataPackage
    where TDp17 : DataPackage
    where TDp18 : DataPackage
;
/// <summary>
/// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>
/// </summary>
public sealed class DataPackageCollection<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13, TDp14, TDp15, TDp16, TDp17, TDp18, TDp19>(IServiceProvider provider)
: DataPackageCollection(provider, typeof(TDp1), typeof(TDp2), typeof(TDp3), typeof(TDp4), typeof(TDp5), typeof(TDp6), typeof(TDp7), typeof(TDp8), typeof(TDp9), typeof(TDp10), typeof(TDp11), typeof(TDp12), typeof(TDp13), typeof(TDp14), typeof(TDp15), typeof(TDp16), typeof(TDp17), typeof(TDp18), typeof(TDp19))
    where TDp1 : DataPackage
    where TDp2 : DataPackage
    where TDp3 : DataPackage
    where TDp4 : DataPackage
    where TDp5 : DataPackage
    where TDp6 : DataPackage
    where TDp7 : DataPackage
    where TDp8 : DataPackage
    where TDp9 : DataPackage
    where TDp10 : DataPackage
    where TDp11 : DataPackage
    where TDp12 : DataPackage
    where TDp13 : DataPackage
    where TDp14 : DataPackage
    where TDp15 : DataPackage
    where TDp16 : DataPackage
    where TDp17 : DataPackage
    where TDp18 : DataPackage
    where TDp19 : DataPackage
;
/// <summary>
/// Combines all <see cref="DataPackage"/>s included in its Type Arguments into one <see cref="DataPackageCollection"/>
/// </summary>
public sealed class DataPackageCollection<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13, TDp14, TDp15, TDp16, TDp17, TDp18, TDp19, TDp20>(IServiceProvider provider)
: DataPackageCollection(provider, typeof(TDp1), typeof(TDp2), typeof(TDp3), typeof(TDp4), typeof(TDp5), typeof(TDp6), typeof(TDp7), typeof(TDp8), typeof(TDp9), typeof(TDp10), typeof(TDp11), typeof(TDp12), typeof(TDp13), typeof(TDp14), typeof(TDp15), typeof(TDp16), typeof(TDp17), typeof(TDp18), typeof(TDp19), typeof(TDp20))
    where TDp1 : DataPackage
    where TDp2 : DataPackage
    where TDp3 : DataPackage
    where TDp4 : DataPackage
    where TDp5 : DataPackage
    where TDp6 : DataPackage
    where TDp7 : DataPackage
    where TDp8 : DataPackage
    where TDp9 : DataPackage
    where TDp10 : DataPackage
    where TDp11 : DataPackage
    where TDp12 : DataPackage
    where TDp13 : DataPackage
    where TDp14 : DataPackage
    where TDp15 : DataPackage
    where TDp16 : DataPackage
    where TDp17 : DataPackage
    where TDp18 : DataPackage
    where TDp19 : DataPackage
    where TDp20 : DataPackage
;

#endregion