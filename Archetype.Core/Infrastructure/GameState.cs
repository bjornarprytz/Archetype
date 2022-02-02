using Archetype.Core.Atoms;
using Archetype.View.Atoms;
using Archetype.View.Infrastructure;

namespace Archetype.Core.Infrastructure;

public interface IGameState : IGameStateFront
{
    new IPlayer Player { get; }
    new IMap Map { get; }
}
    
internal class GameState : IGameState
{
    public GameState(IMap map, IPlayer player)
    {
        Map = map;
        Player = player;
    }

    public IPlayer Player { get; }
    IMapFront IGameStateFront.Map => Map;
    IPlayerFront IGameStateFront.Player => Player;

    public IMap Map { get; }
}