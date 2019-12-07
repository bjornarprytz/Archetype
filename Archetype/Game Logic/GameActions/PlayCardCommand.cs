using System;
using System.Windows.Input;

namespace Archetype
{
    public class PlayCardCommand : ICommand
    {
        private readonly Action<Timeline> _action;
        public PlayCardCommand(Card cardToPlay, PlayCardArgs args)
        {
            _action = (timeline) => cardToPlay.Play(args, timeline);
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            if (!(parameter is Timeline)) throw new ArgumentException($"{parameter} must be {typeof(Timeline)}");

            _action(parameter as Timeline);
        }
    }
}
