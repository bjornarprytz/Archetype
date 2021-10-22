using System.Threading.Tasks;

namespace Archetype.Core
{
    public interface ICard : IGamePiece
    {
        CardData Data { get; }
        
        IZone CurrentZone { get; }
    }
}
