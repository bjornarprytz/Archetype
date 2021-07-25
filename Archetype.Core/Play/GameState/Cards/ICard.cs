using System.Threading.Tasks;

namespace Archetype.Core
{
    public interface ICard
    {
        CardData Data { get; set; }

        ICardArgs GetPlayArgs();
        bool ValidateArgs(ICardArgs args, IGameState gameState);
        Task ResolveAsync(ICardArgs args, IGameState gameState);
    }
}
