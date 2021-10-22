using System.Threading.Tasks;

namespace Archetype.Core
{
    public interface ICard
    {
        CardData Data { get; set; }

        ICardArgs GenerateArgs();
        Task ResolveAsync(ICardArgs args, IGameState gameState);
    }
}
