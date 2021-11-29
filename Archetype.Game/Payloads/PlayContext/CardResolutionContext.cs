using System.Collections.Generic;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.PlayContext
{
    public interface ICardResolutionContext
    {
        [Target("Player")] // Maybe this should have another attribute
        IGameAtom Source { get; }
        IEnumerable<IGameAtom> Targets { get; }
        
        [Target("World")] // Maybe this should have another attribute
        IGameState GameState { get; }
    }
    
    public class CardResolutionContext : ICardResolutionContext
    {
        public CardResolutionContext(IGameState gameState, IGameAtom source, IEnumerable<IGameAtom> targets)
        {
            GameState = gameState;
            Source = source;
            Targets = targets;
        }
        
        public IGameState GameState { get; }
        public IGameAtom Source { get; }
        public IEnumerable<IGameAtom> Targets { get; }
    }
}