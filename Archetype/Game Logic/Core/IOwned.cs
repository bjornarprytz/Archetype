
namespace Archetype
{
    public interface IOwned<T> where T : GamePiece
    {
        T Owner { get; set; }
    }
}
