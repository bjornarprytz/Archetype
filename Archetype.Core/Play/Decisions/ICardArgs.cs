using System.Collections.Generic;

namespace Archetype.Core
{
    public interface ICardArgs
    {
        IList<IEffectArgs> EffectArgs { get; set; }
    }
}
