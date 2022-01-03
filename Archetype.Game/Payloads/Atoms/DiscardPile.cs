using System.Collections.Generic;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;

namespace Archetype.Game.Payloads.Atoms
{
    [Target("Discard Pile")]
    public interface IDiscardPile : IZone<ICard>, IDiscardPileFront
    { }
    
    internal class DiscardPile : Zone<ICard>, IDiscardPile
    {
        public DiscardPile(IGameAtom owner) : base(owner) { }

        public IEnumerable<ICardFront> Cards => Contents;
    }
}
