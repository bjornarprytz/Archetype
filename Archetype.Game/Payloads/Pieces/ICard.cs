using System.Collections.Generic;
using Archetype.Core;
using Archetype.Game.Payloads.Metadata;

namespace Archetype.Game.Payloads.Pieces
{
    public interface ICard : IGamePiece
    {
        int Cost { get; }
        void AffectSomehow(int x);
        
        IZone CurrentZone { get; }
        
        IList<ITarget> Targets { get; }
        IList<IEffect> Effects { get; }

        CardData CreateReadonlyData();
    }
}
