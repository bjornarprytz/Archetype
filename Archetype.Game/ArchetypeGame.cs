using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Archetype.Core;

namespace Archetype.Game
{
    public class ArchetypeGame : IArchetypeGame
    {
        private readonly IGameState _gameState;
        private readonly Subject<IGameState> _gameStateSubject = new Subject<IGameState>();
        
        public ArchetypeGame(IGameState gameState)
        {
            _gameState = gameState;
        }

        public IObservable<IGameState> GameState => _gameStateSubject;
        
        public async Task PlayCardAsync(IPlayCardArgs playCardArgs)
        {
            await playCardArgs.Card.ResolveAsync(playCardArgs.Args, _gameState);

            _gameStateSubject.OnNext(_gameState);
        }

        public Task EndTurnAsync()
        {
            throw new NotImplementedException();
        }
    }
}