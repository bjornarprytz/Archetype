

namespace Archetype
{
    public interface IOwned<O> where O : GamePiece
    {
        O Owner { get; set; }
    }
}
