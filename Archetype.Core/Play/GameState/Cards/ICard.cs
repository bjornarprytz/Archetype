using System.Threading.Tasks;

namespace Archetype.Core
{
    public interface ICard
    {
        CardData Data { get; set; }

        ICardArgs GetPlayArgs();
        Task ResolveAsync(ICardArgs args, IGameState gameState);
    }
}
