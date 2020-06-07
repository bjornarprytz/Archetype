using System;
using System.Windows.Input;

namespace Archetype
{
    public class PlayCardAction : IGameAction
    {
        private readonly Action<GameState> _action;
        private readonly Unit _player;
        private readonly PlayCardArgs _args;

        public PlayCardAction(Unit player, Card cardToPlay, PlayCardArgs args)
        {
            _action = (gameState) => cardToPlay.Play(args, gameState);
            _player = player;
            _args = args;
        }

        public bool CanExecute(GameState gameState) => gameState.HasTurn(_player) &&  _args.Valid;

        public void Execute(GameState gameState) => _action(gameState);
    }
}
