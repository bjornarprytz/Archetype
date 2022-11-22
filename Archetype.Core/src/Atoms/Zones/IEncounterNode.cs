namespace Archetype.Core.Atoms.Zones;

public interface IEncounterNode : IZone<ICard>
{
    public IEnumerable<IEncounterNode> Neighbors { get; }
}