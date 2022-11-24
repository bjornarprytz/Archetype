using Archetype.Core.Atoms;

namespace Archetype.Core.Extensions;

public static class AtomExtensions
{
    public static bool IsFriendly(this IAtom atom) => atom.TopOwner() is IPlayer;

    public static bool IsEnemy(this IAtom atom) => !atom.IsFriendly();

    public static bool IsFriendOf(this IAtom atom, IAtom other)
    {
        return atom.TopOwner() == other.TopOwner();
    }
    public static bool IsEnemyOf(this IAtom atom, IAtom other) => !atom.IsFriendOf(other);
        
    public static IAtom TopOwner(this IAtom atom)
    {
        var owner = atom.Owner;
            
        while (owner is not null && owner.Owner != owner)
        {
            owner = owner.Owner;
        }

        return owner;
    }
}