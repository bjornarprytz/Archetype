namespace Archetype.Framework.Runtime.State;

public class GameState : IGameState
{
    public IDictionary<Guid, IZone> Zones { get; set; }
    public IDictionary<Guid, IAtom> Atoms { get; set; }
}