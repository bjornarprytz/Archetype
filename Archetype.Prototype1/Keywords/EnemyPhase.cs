using Archetype.Framework.Core.Primitives;

namespace Archetype.Prototype1.Keywords;

public class EnemyPhaseResolver : EffectCompositeDefinition
{
    public override string Name { get; } = "Enemy Phase";
    public override string ReminderText { get; } = "Resolve an encounter.";
    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        throw new NotImplementedException();
    }
}