using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.Actions;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Definitions;

public interface IKeywordDefinition
{
    string Name { get; }
    string ReminderText { get; }
    IReadOnlyList<KeywordTargetDescription> Targets { get; }
    IReadOnlyList<IOperandDescription> Operands { get; }
}
public abstract class KeywordDefinition : IKeywordDefinition
{
    public abstract string Name { get; } // ID
    public abstract string ReminderText { get; } // E.g. "Deal [X] damage to target unit or structure"
    protected virtual OperandDeclaration OperandDeclaration { get; } = new();
    protected virtual TargetDeclaration TargetDeclaration { get; } = new();
    public IReadOnlyList<KeywordTargetDescription> Targets => TargetDeclaration;
    public IReadOnlyList<IOperandDescription> Operands => OperandDeclaration;
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
    public abstract IEvent Resolve(IResolutionContext context, EffectPayload effectPayload);
}

public abstract class EffectCompositeDefinition : KeywordDefinition
{
    public abstract ICompositeKeywordInstance CreateEffectSequence(
        IResolutionContext context, 
        IKeywordInstance keywordInstance
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
public abstract class CostDefinition : EffectCompositeDefinition
{
    public CostType Type { get; set; }
    
    public abstract bool Check(PaymentPayload paymentPayload, KeywordInstance keywordInstance);
}

// Examples:
// N_CARDS_IN_HAND 'X'
// Will compute the cards in hand and store it in the 'X' property
//
// Other definitions can then use the computed value:
// RESOURCE_COST [X]
public abstract class ComputedValueDefinition : KeywordDefinition
{
    public abstract int Compute(IAtom source, IGameState gameState);
}

