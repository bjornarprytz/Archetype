using System;
using System.Collections;
using System.Runtime.InteropServices.ComTypes;

namespace Archetype.CardBuilder
{
    public class InvalidTargetIndexException : Exception
    {
        public InvalidTargetIndexException(int index, int targetCount)
        {
            
            Message = $"Invalid target index {index} for target count {targetCount}";
        }

        public override string Message { get; }
    }
    
    public class MissingResolutionFunctionException : Exception
    {
        public MissingResolutionFunctionException()
        {
            
            Message = $"Effect is missing a resolve function";
        }

        public override string Message { get; }
    }
}