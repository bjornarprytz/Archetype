using System;

namespace Archetype.Core
{
    public interface IEffectMetaData
    {
        Type TargetType { get; }
        Type ResultType { get; }
        
        string ValidationFunctionName { get; }
        string ResolutionFunctionName { get; }
    }
}
