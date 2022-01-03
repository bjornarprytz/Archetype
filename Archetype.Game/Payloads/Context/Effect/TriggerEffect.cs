using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context.Effect.Base;
using Archetype.Game.Payloads.Context.Trigger;

namespace Archetype.Game.Payloads.Context.Effect
{
    internal class TriggerEffect<TSource> : Effect<ITriggerContext<TSource>, IResult>
        where TSource : IGameAtom
    {
    }
}