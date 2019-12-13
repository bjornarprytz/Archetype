using System;
using System.Windows.Input;

namespace Archetype.Game_Logic.GameActions
{
    public class EndTurnCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action<GameLoop> _action;

        public EndTurnCommand(Unit unit)
        {
            _action = (gameLoop) => gameLoop.EndTurn(unit);
        }

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            if (!(parameter is GameLoop)) throw new ArgumentException($"{parameter} must be {typeof(GameLoop)}");

            _action(parameter as GameLoop);
        }
    }
}
