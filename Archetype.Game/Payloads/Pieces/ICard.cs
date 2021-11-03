using System.Collections.Generic;
using Archetype.Game.Payloads.Metadata;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces
{
    public interface ICard : IGamePiece
    {
        ICardProtoData ProtoData { get; }
        int Cost { get; }
        void AffectSomehow(int x);
        
        IZone CurrentZone { get; }
        
        IEnumerable<ITarget> Targets { get; }
        IEnumerable<IEffect> Effects { get; }
    }
}
