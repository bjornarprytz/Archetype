namespace Archetype
{
    public class CommandHandler
    {
        private IGameAction _action { get; set; }

        public CommandHandler(IGameAction gameAction)
        {
            _action = gameAction;
        }

    }
}
