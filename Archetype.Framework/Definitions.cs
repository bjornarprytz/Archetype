using System.Text.RegularExpressions;

namespace Archetype.Framework;

public abstract class KeywordDefinition
{
    public string Name { get; set; } // ID
    public KeywordType Type { get; set; }
    public string Template { get; set; }
    public string ReminderText { get; set; }
    public Regex Pattern { get; set; }
    public ParseKeyword Parse { get; set; }
}

// The bread and butter of state changes
public class EffectDefinition : KeywordDefinition
{
    public ResolveEffect Resolve { get; set; }
    public int NArgs { get; set; } // All args are integers
    public IEnumerable<CardType> Targets { get; set; }
}

// Hook into special rules
public class FeatureDefinition : KeywordDefinition
{
    public bool Stackable { get; set; }
}

// Modify game objects
public class AuraDefinition : KeywordDefinition
{
    public Scope Scope { get; set; }
    public ApplyAura Apply { get; set; }
    public RemoveAura Remove { get; set; }
    public CheckCard CanApply { get; set; }
    public int Duration { get; set; } = -1; // -1 = permanent
}

public class ReactionDefinition : KeywordDefinition
{
    public CheckEvent WillTrigger { get; set; }
    public int Count { get; set; } = -1; // -1 = infinite
}

public class AbilityDefinition : KeywordDefinition
{
    
}

// Conditions for using an ability or playing a card
public class ConditionDefinition : KeywordDefinition
{
    public CheckState Check { get; set; }
}

// Cost of abilities and cards
public class CostDefinition : KeywordDefinition
{
    public CostType Type { get; set; }
    public CheckCost Check { get; set; }
    public ResolveCost Resolve { get; set; }
}
