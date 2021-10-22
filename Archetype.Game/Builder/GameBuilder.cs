namespace Archetype.Game
{
    public class GameBuilder
    {
        private readonly GameBuilderOptions _options;
        
        public GameBuilder(GameBuilderOptions options)
        {
            _options = options;
        }

        IArchetypeGame Build()
        {
            // TODO: Use options

            return new ArchetypeGame(new GameState());
        }
    }
}