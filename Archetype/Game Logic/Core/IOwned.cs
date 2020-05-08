
namespace Archetype
{
    public interface IOwned<OwnerType> where OwnerType : GamePiece
    {
        OwnerType Owner { get; }
    }
}
