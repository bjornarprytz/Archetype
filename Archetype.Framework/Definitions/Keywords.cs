
using System.Text.RegularExpressions;
using Archetype.Core;
using Archetype.Rules.Proto;

namespace Archetype.Rules.Definitions;

public class KeywordOperand
{
    public KeywordOperandType Type { get; set; }
    public string Description { get; set; }
    public bool IsOptional { get; set; }
}

public class KeywordTarget
{
    public string Type { get; set; }
    public string Description { get; set; }
    public bool IsOptional { get; set; }
}

public abstract class KeywordDefinition
{
    public string Name { get; set; } // ID
    public string ReminderText { get; set; }
    public Regex Pattern { get; set; }
    public Func<string, KeywordInstance> Parse { get; set; }
    public IReadOnlyList<KeywordTarget> Targets { get; set; }
    public IReadOnlyList<KeywordOperand> Operands { get; set; }
}

// The bread and butter of state changes
// Examples:
// DAMAGE <1> 6
// HEAL <2> 3
// DRAW 4
// MODIFY <1> Strength 1
public class EffectDefinition : KeywordDefinition
{
    public ResolveEffect Resolve { get; set; }
}

// Hook into special rules
//
// Examples:
// STRENGTH 2
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

