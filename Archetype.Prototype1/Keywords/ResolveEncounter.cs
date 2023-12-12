using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Meta;

namespace Archetype.Prototype1.Keywords;

[Keyword("RESOLVE_ENCOUNTER")]
public class ResolveEncounter : EffectCompositeDefinition
{
    public override string ReminderText { get; } = "Resolve an encounter.";
    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        throw new NotImplementedException();
    }
}