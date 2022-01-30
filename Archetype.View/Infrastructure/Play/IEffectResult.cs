using Archetype.View.Atoms;

namespace Archetype.View.Infrastructure;

public interface IEffectResult
{
    bool IsNull { get; }
    IEnumerable<IGameAtomFront> AllAffected { get; }
    string Verb { get; }
    object Result { get; }
}