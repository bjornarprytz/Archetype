using System.Linq;
using Archetype.Core.Data.Composite;
using Archetype.Game.Payloads;

namespace Archetype.Server
{
    public class Query
    {
        private readonly IGameState _gameState;

        public Query(IGameState gameState)
        {
            _gameState = gameState;
        }

        public GameStateData GetGameState()
        {
            var player  = new PlayerData
                {
                    Resources = _gameState.Player.Resources,
                    DiscardPile = _gameState.Player.DiscardPile.Cards.Select(c => c.CreateReadonlyData()).ToList(),
                    Hand = _gameState.Player.Hand.Cards.Select(c => c.CreateReadonlyData()).ToList()
                }; 
            
            
            var gameState = new GameStateData { Player = player };
            
            return gameState;
        } 
    }
}