using System;

namespace Archetype.Builder.Exceptions;

public class InvalidTargetIndexException : Exception
{
    public InvalidTargetIndexException(int index, int targetCount)
    {
            
        Message = $"Invalid target index {index} for target count {targetCount}";
    }

    public override string Message { get; }
}
    
public class MissingResolutionExpressionException : Exception { }
public class DisconnectedNodesException : Exception { }

public class MissingNameException : Exception { }