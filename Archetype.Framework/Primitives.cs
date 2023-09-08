namespace Archetype.Framework;

public enum CardType
{
    Spell,
    Unit,
    Structure,
    Node,
    Enemy
}

public enum KeywordType
{
    Effect,
    Feature,
    Aura,
    Reaction,
    DelayedEffect,
    Ability,
    Condition,
    Cost
}

public enum CostType
{
    Resource, // From hand, with resource value
    Sacrifice, // From the board
    Discard, // From hand
    Mill, // From draw pile
    Exhaust, // From discard pile
}

public enum Scope
{
    Self,
    Node,
    Global
}

