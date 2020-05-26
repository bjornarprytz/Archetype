
namespace Archetype
{
    public interface IGameAction
    {
        bool CanExecute(GameState gameState);

        void Execute(GameState gameState);
    }
}
