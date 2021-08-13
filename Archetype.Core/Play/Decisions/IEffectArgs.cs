
using System;
using System.Collections.Generic;

namespace Archetype.Core
{
    public interface IEffectArgs
    {
        IList<Type> AllowedTypes { get; }
        
        int MinTargets { get; }
        int MaxTargets { get; }
        IList<ITarget> Targets { get; set; }
    }
}
