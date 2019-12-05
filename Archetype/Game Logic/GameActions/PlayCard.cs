using System;
using System.Windows.Input;

namespace Archetype
{
    public class PlayCard : ICommand
    {
        private Action _action;
        public PlayCard(Card cardToPlay, CardArgs args, Timeline timeline)
        {
            _action = () => cardToPlay.Play(args, timeline);
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            _action();
        }
    }
}
