using Archetype.Framework.Core.Structure;
using Archetype.Framework.State;

namespace Archetype.Prototype1;

public class GameState : IGameState
{
    private readonly Dictionary<Guid, IZone> _zones = new();
    private readonly Dictionary<Guid, IAtom> _atoms = new();
    
    public IReadOnlyDictionary<Guid, IZone> Zones => _zones;
    public IReadOnlyDictionary<Guid, IAtom> Atoms => _atoms;
    public IPlayer Player { get; } = new Player();
    public void AddAtom(IAtom atom)
    {
        _atoms.Add(atom.Id, atom);
    }

    public void AddZone(IZone zone)
    {
        _zones.Add(zone.Id, zone);
    }
}