using Archetype.Framework.Core.Primitives;

namespace Archetype.Prototype1.Keywords;

public class ResolveEncounter : EffectCompositeDefinition
{
    public override string Name { get; } = "Resolve Encounter";
    public override string ReminderText { get; } = "Resolve an encounter.";
    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        throw new NotImplementedException();
    }
}