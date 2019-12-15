using System;

namespace Archetype
{
    public class EndTurnAction : IGameAction
    {
        private readonly Action<GameLoop> _action;

        public EndTurnAction(Unit unit)
        {
            _action = (gameLoop) => gameLoop.EndTurn(unit);
        }

        public bool CanExecute(GameLoop gameLoop) => true;

        public void Execute(GameLoop gameLoop) => _action(gameLoop);
    }
}
