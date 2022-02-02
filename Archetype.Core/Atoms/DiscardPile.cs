using Archetype.Core.Atoms.Base;
using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;

namespace Archetype.Core.Atoms;

public interface IDiscardPile : IZone<ICard>, IDiscardPileFront
{ }
    
internal class DiscardPile : Zone<ICard>, IDiscardPile
{
    public DiscardPile(IGameAtom owner) : base(owner) { }

    public IEnumerable<ICardFront> Cards => Contents;
}