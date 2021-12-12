using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IGraveyard : IZone<IUnit> // TODO: This should be of ICreature, but I might have to do something smarter with the interfaces
    { }

    public class Graveyard : Zone<IUnit>, IGraveyard
    {
        public Graveyard(IGameAtom owner) : base(owner) { }
    }
}