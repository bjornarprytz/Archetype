using System;

namespace Archetype
{
    public class EndTurnAction : IGameAction
    {
        private readonly Action<GameState> _action;
        private readonly Unit _unit;

        public EndTurnAction(Unit unit)
        {
            _action = (gameLoop) => gameLoop.EndTurn(unit);
            _unit = unit;
        }

        public bool CanExecute(GameState gameState) => gameState.HasTurn(_unit);

        public void Execute(GameState gameState) => _action(gameState);
    }
}
