using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLib.Exceptions.CommonExceptions;
public class MaxIterationDepthReachedException : JLibException
{
    public int MaxDepth { get; }

    public MaxIterationDepthReachedException(int maxDepth)
        : base($"The maximum iteration depth of {maxDepth} was reached. This is usually a sign of a circular reference.")
    {
        MaxDepth = maxDepth;
        Data[nameof(MaxDepth)] = maxDepth;
    }
}
