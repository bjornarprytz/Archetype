using System;

namespace Archetype.Core
{
    public interface IEffectMetaData
    {
        Type TargetType { get; }
        Type ResultType { get; }
        
        int TargetIndex { get; }
        
        string RulesTextFunctionName { get; }
        string ResolutionFunctionName { get; }
    }
}
