
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Extensions
{
    internal static class AtomExtensions
    {
        public static bool IsFriendly(this IGameAtom atom) => atom.TopOwner() is IPlayer;

        public static bool IsEnemy(this IGameAtom atom) => !atom.IsFriendly();

        public static bool IsFriendOf(this IGameAtom atom, IGameAtom other)
        {
            return atom.TopOwner() == other.TopOwner();
        }
        public static bool IsEnemyOf(this IGameAtom atom, IGameAtom other) => !atom.IsFriendOf(other);
        
        public static IGameAtom TopOwner(this IGameAtom atom)
        {
            var owner = atom.Owner;
            
            while (owner is not null && owner.Owner != owner)
            {
                owner = owner.Owner;
            }

            return owner;
        }
    }
}