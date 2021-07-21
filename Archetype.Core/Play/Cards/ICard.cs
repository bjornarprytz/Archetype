using System.Threading.Tasks;

namespace Archetype.Core
{
    public interface ICard
    {
        CardData Data { get; set; }

        CardPlayArgs GetPlayArgs();
        bool ValidateArgs(CardPlayArgs args, IGameState gameState);
        Task ResolveAsync(CardPlayArgs args, IGameState gameState);
    }
}
