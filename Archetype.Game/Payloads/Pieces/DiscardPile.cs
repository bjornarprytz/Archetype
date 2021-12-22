using System;
using System.Collections.Generic;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IDiscardPileFront : IZoneFront
    {
        IEnumerable<ICardFront> Cards { get; }
    }

    [Target("Discard Pile")]
    internal interface IDiscardPile : IZone<ICard>, IDiscardPileFront
    { }
    
    internal class DiscardPile : Zone<ICard>, IDiscardPile
    {
        public DiscardPile(IGameAtom owner) : base(owner) { }

        public IEnumerable<ICardFront> Cards => Contents;
    }
}
