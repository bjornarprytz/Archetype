
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Extensions
{
    public static class AtomExtensions
    {
        public static bool IsFriendly(this IGameAtom atom)
        {
            var owner = atom.Owner;
            
            while (owner is not null && owner.Owner != owner)
            {
                owner = owner.Owner;
            }

            return owner is IPlayer;
        }

        public static bool IsEnemy(this IGameAtom atom) => !atom.IsFriendly();
    }
}