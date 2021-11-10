using System.Collections.Generic;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.PlayContext
{
    public interface ICardResolutionContext
    {
        IGameAtom Source { get; }
        IEnumerable<IGameAtom> Targets { get; }
        
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