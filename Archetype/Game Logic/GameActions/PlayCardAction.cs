using System;
using System.Windows.Input;

namespace Archetype
{
    public class PlayCardAction : IGameAction
    {
        private readonly Action<GameLoop> _action;
        public PlayCardAction(Card cardToPlay, PlayCardArgs args)
        {
            _action = (gameLoop) => cardToPlay.Play(args, gameLoop);
        }

        public bool CanExecute(GameLoop gameLoop) => true; // TODO: Figure out whether the action can be taken

        public void Execute(GameLoop gameLoop) => _action(gameLoop);
    }
}
