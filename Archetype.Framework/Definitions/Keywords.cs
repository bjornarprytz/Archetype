using System.Collections;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;

namespace Archetype.Framework.Definitions;

public record OperandDescription(KeywordOperandType Type, bool IsOptional);
public record TargetDescription(IReadOnlyDictionary<string, string> Characteristics, bool IsOptional);

public abstract class KeywordDefinition
{
    public abstract string Name { get; } // ID
    public abstract string ReminderText { get; } // E.g. "Deal [X] damage to target unit or structure"
    public virtual IReadOnlyList<TargetDescription> Targets { get; } = Array.Empty<TargetDescription>();
    public virtual IReadOnlyList<OperandDescription> Operands { get; } = Array.Empty<OperandDescription>();
}

// The bread and butter of state changes
// Examples:
// DAMAGE <1> 6
// HEAL <2> 3
// DRAW [X] // get args from a computed value
// MODIFY <1> Strength 1
// DISCARD -1- // get args from the first prompt response
public abstract class EffectPrimitiveDefinition : KeywordDefinition
{
    public abstract IEvent Resolve(IResolutionContext context, Effect payload);
}

public abstract class EffectCompositeDefinition : KeywordDefinition
{
    public abstract IEnumerable<EffectInstance> CreateEffectSequence(
        IResolutionContext context,
        IDefinitions definitions
        );
}

// Hook into special rules
//
// Examples:
// STRENGTH 2
// PROMPT "Choose a card to discard"  
// TARGETS Enemy Unit Any
// 
// Other definitions can then reference the targets:
// DAMAGE <1> 6 // Deal 6 damage to the first target
// HEAL <2> 3 // Heal the second target for 3
public class FeatureDefinition : KeywordDefinition
{
    
}

// ON_DEATH Self { ... }
public class ReactionDefinition : KeywordDefinition
{
    public CheckEvent CheckIfTriggered { get; set; }
}

// ABILITY { ... }
public class AbilityDefinition : KeywordDefinition
{
    
}

// IN_HAND_CONDITION
// LIFE_GTE_CONDITION 10
public class ConditionDefinition : KeywordDefinition
{
    public CheckState Check { get; set; }
}

// Examples:
// RESOURCE_COST 4
// SACRIFICE_COST 1
public class CostDefinition : KeywordDefinition
{
    public CostType Type { get; set; }
    public CheckCost Check { get; set; }
    public ResolveCost Resolve { get; set; }
}

// Examples:
// N_CARDS_IN_HAND 'X'
// Will compute the cards in hand and store it in the 'X' property
//
// Other definitions can then use the computed value:
// RESOURCE_COST [X]
public class ComputedValueDefinition : KeywordDefinition
{
    public ComputeProperty Compute { get; set; }
}

