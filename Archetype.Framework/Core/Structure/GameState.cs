using Archetype.Framework.State;

namespace Archetype.Framework.Core.Structure;

public interface IGameState
{
    IReadOnlyDictionary<Guid, IZone> Zones { get; }
    IReadOnlyDictionary<Guid, IAtom> Atoms { get; }
    
    IPlayer Player { get; }
    
    void AddAtom(IAtom atom);
    void AddZone(IZone zone);
}