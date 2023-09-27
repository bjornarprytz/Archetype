using System.Collections;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.Actions;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Definitions;

public record OperandDescription(KeywordOperandType Type, bool IsOptional);
public record TargetDescription(Filter Filter, bool IsOptional); 
// This represesnts two things currently, allowed targets for a keyword and the target of an effect
// E.g. "Deal 3 damage to target unit or structure", but Damage can also target a player, so it's not a 1:1 mapping
// TODO: Figure out if this needs to be split into two different things.

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
    public abstract IEvent Resolve(IResolutionContext context, Effect effect);
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
// TODO: Figure out something smart for characteristics and features. The idea is to have low-effort static definitions, while supporting reminder text, computed properties, if possible. 
public abstract class CharacteristicDefinition : KeywordDefinition { } 

// ON_DEATH Self { ... }
public abstract class ReactionDefinition : KeywordDefinition
{ 
    public CheckEvent CheckIfTriggered { get; set; }
}

// ABILITY { ... }
public abstract class AbilityDefinition : KeywordDefinition
{
    
}

// IN_HAND_CONDITION
// LIFE_GTE_CONDITION 10
public abstract class ConditionDefinition : KeywordDefinition
{
    public CheckState Check { get; set; }
}

// Examples:
// RESOURCE_COST 4
// SACRIFICE_COST 1
public abstract class CostDefinition : KeywordDefinition
{
    public CostType Type { get; set; }
    
    public abstract IEvent Resolve(IGameState gameState, IDefinitions definitions, CostPayload costPayload);
    public abstract bool Check(CostPayload costPayload, int amount);
}

// Examples:
// N_CARDS_IN_HAND 'X'
// Will compute the cards in hand and store it in the 'X' property
//
// Other definitions can then use the computed value:
// RESOURCE_COST [X]
public abstract class ComputedValueDefinition : KeywordDefinition
{
    public abstract object Compute(IAtom source, IGameState gameState);
}

