using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Meta;

namespace Archetype.Prototype1.Keywords;

[EffectKeyword("ENEMY_PHASE")]
public class EnemyPhaseResolver : EffectCompositeDefinition
{
    public override string ReminderText { get; } = "Resolve an encounter.";
    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        throw new NotImplementedException();
    }
}