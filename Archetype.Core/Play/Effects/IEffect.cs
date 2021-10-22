using System;

namespace Archetype.Core
{
    public interface IEffect
    {
        Type TargetType { get; }
        Type ResultType { get; }
    }
}
