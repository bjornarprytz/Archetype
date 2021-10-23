using System;

namespace Archetype.Core
{
    public interface IEffectMetaData
    {
        Type TargetType { get; }
        Type ResultType { get; }
        
        
        string RulesTextFunctionName { get; }
        string ValidationFunctionName { get; }
        string ResolutionFunctionName { get; }
    }
}
