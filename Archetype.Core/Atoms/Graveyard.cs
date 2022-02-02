using Archetype.Core.Atoms.Base;
using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;

namespace Archetype.Core.Atoms;

public interface IGraveyard : IZone<IUnit>, IGraveyardFront // TODO: This should be of ICreature, but I might have to do something smarter with the interfaces
{ }

internal class Graveyard : Zone<IUnit>, IGraveyard
{
    public Graveyard(IGameAtom owner) : base(owner) { }
    public IEnumerable<ICreatureFront> Creatures => Contents.OfType<ICreature>();
}