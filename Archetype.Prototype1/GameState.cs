using Archetype.Framework.Core.Structure;
using Archetype.Framework.State;

namespace Archetype.Prototype1;

public class GameState : IGameState
{
    public IDictionary<Guid, IZone> Zones { get; } = new Dictionary<Guid, IZone>();
    public IDictionary<Guid, IAtom> Atoms { get; } = new Dictionary<Guid, IAtom>();
    public IPlayer Player { get; } = new Player();
}