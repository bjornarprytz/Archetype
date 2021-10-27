using System.Threading.Tasks;

namespace Archetype.Core
{
    public interface ICard : IGamePiece
    {
        CardData Data { get; }

        void AffectSomehow(int x);
        
        IZone CurrentZone { get; }
    }
}
