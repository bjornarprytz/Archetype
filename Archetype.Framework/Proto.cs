namespace Archetype.Framework;

public class ProtoEffect
{
    public CreateEffect Create { get; set; }
}

public class ProtoReaction
{
    public ReactionDefinition Definition { get; set; }
    public ProtoEffect Effect { get; set; }
}

public class ProtoAura
{
    public ProtoCondition Condition { get; set; }
    public AuraDefinition Definition { get; set; }
}

public class ProtoFeature
{
    public string Name { get; set; }
    public int Stacks { get; set; }
}

public class ProtoAbility
{
    public IEnumerable<ProtoCondition> Conditions { get; set; }
    public ProtoCost Cost { get; set; }
    public IEnumerable<ProtoEffect> Effects { get; set; }
    
    public CreateAbilityEffects CreateEffects { get; set; }
}

public class ProtoCondition
{
    public CheckState Check { get; set; }
}

public class ProtoCost
{
    public CostDefinition Definition { get; set; }
}

public class ProtoCard
{
    public string Name { get; set; } // ID
    public CardType Type { get; set; }
    public ProtoCost Cost { get; set; }
    public IEnumerable<ProtoCondition> Conditions { get; set; }
    public IEnumerable<ProtoReaction> Reactions { get; set; }
    public IEnumerable<ProtoEffect> Effects { get; set; }
    public IEnumerable<ProtoAura> Auras { get; set; }
    public IEnumerable<ProtoFeature> Features { get; set; }
    public IEnumerable<ProtoAbility> Abilities { get; set; }
    
    public CreateCardEffects CreateEffects { get; set; }

    // TODO: Basically add functionality to cover all the keyword definitions
}
