using System;

namespace Archetype.Core
{
    public interface ITargetMetaData
    {
        Type TargetType { get; }
        
        string ValidationFunctionName { get; }
    }
}