using Archetype.Framework.Core.Structure;
using Archetype.Framework.State;

namespace Archetype.Framework.Extensions;

public static class GameStateExtensions
{
    public static T GetAtom<T> (this IGameState gameState, Guid id) where T : IAtom
    {
        if (!gameState.Atoms.TryGetValue(id, out var atom))
            throw new InvalidOperationException($"Atom ({id}) not found");

        if (atom is T requiredAtom)
            return requiredAtom;
        
        throw new InvalidOperationException($"Atom ({id}) is not a {typeof(T).Name}");
    }
    
    public static IAtom GetAtom(this IGameState gameState, Guid id)
    {
        if (gameState.Atoms.TryGetValue(id, out var atom))
            return atom;
        
        throw new InvalidOperationException($"Atom ({id}) not found");
    }
}