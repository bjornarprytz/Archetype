using System;
using System.Windows.Input;

namespace Archetype
{
    public class PlayCardAction : IGameAction
    {
        private readonly Action<GameLoop> _action;
        private readonly Unit _player;
        private readonly PlayCardArgs _args;

        public PlayCardAction(Unit player, Card cardToPlay, PlayCardArgs args)
        {
            _action = (gameLoop) => cardToPlay.Play(args, gameLoop);
            _player = player;
            _args = args;
        }

        public bool CanExecute(GameLoop gameLoop) => gameLoop.HasTurn(_player) &&  _args.Valid;

        public void Execute(GameLoop gameLoop) => _action(gameLoop);
    }
}
