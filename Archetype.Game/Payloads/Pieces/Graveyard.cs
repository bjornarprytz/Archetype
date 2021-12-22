using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IGraveyardFront : IZoneFront
    {
        IEnumerable<ICreatureFront> Creatures { get; }
    }
    
    internal interface IGraveyard : IZone<IUnit>, IGraveyardFront // TODO: This should be of ICreature, but I might have to do something smarter with the interfaces
    { }

    internal class Graveyard : Zone<IUnit>, IGraveyard
    {
        public Graveyard(IGameAtom owner) : base(owner) { }
        public IEnumerable<ICreatureFront> Creatures => Contents.OfType<ICreature>();
    }
}