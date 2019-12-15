
namespace Archetype
{
    public interface IGameAction
    {
        bool CanExecute(GameLoop gameLoop);

        void Execute(GameLoop gameLoop);
    }
}
