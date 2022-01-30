using Archetype.View.Atoms;

namespace Archetype.View.Infrastructure;

public interface IEffectResult
{
    IEnumerable<IEffectResult> SideEffects { get; }

    string Verb { get; }
    bool IsNull { get; }
    IGameAtomFront Affected { get; }
    object Result { get; }
}