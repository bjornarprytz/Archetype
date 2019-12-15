using System;

namespace Archetype
{
    public class EndTurnAction : IGameAction
    {
        private readonly Action<GameLoop> _action;
        private readonly Unit _unit;

        public EndTurnAction(Unit unit)
        {
            _action = (gameLoop) => gameLoop.EndTurn(unit);
            _unit = unit;
        }

        public bool CanExecute(GameLoop gameLoop) => gameLoop.HasTurn(_unit);

        public void Execute(GameLoop gameLoop) => _action(gameLoop);
    }
}
