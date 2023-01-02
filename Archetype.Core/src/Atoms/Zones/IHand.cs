using Archetype.Core.Atoms.Cards;

namespace Archetype.Core.Atoms.Zones;

public interface IHand : IZone<ICard>
{
    public int Capacity { get; set; }
}