using System.Collections;
using System.Collections.Generic;
using Archetype.Game.Payloads.Context.Effect;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Context.Trigger
{
    public interface ITriggerSource<in TSource>
        where TSource : IGameAtom
    {
        IEnumerable<IEffect<ITriggerContext<TSource>>> Effects { get; }
    }
    
}