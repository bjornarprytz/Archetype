using System.Collections.Generic;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Payloads.Metadata
{
    public interface IEffect
    {
        int TargetIndex { get; }
        
        public object ResolveContext(ICardResolutionContext context);
        public string CallTextMethod(ICardResolutionContext context);
    }
}
