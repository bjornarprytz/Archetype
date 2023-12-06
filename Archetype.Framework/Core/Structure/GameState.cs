using Archetype.Framework.State;

namespace Archetype.Framework.Core.Structure;

public interface IGameState
{
    IDictionary<Guid, IZone> Zones { get; }
    IDictionary<Guid, IAtom> Atoms { get; }
    
    IPlayer Player { get; }
}