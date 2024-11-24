namespace Archetype.Framework.Data;

public record CardData
{
    public required string Name { get; init; }
    
    /// <summary>
    /// E.g.
    /// <br/>
    /// "Damage" -> 3
    /// "Power" -> 2
    /// "Health" -> 4 
    /// </summary>
    public required Dictionary<string, ReadExpression> Stats { get; init; }
    /// <summary>
    /// E.g.
    /// <br/>
    /// "Types" -> ["Enchantment", "Creature"]
    /// <br/>
    /// "Subtypes" -> ["Human", "Warrior"]
    /// </summary>
    public required Dictionary<string, string[]> Facets { get; init; }
    
    /// <summary>
    /// E.g.
    /// <br/>
    /// ["Trample", "Flying"]
    /// </summary>
    public required string[] Tags { get; init; }
    
    /// <summary>
    /// E.g.
    /// <br/>
    /// "R" -> 1
    /// "G" -> 1
    /// "C" -> 5
    /// </summary>
    public required Dictionary<string, ReadExpression> Costs { get; init; }
    
    public required TargetData[] Targets { get; init; }
    
    /// <summary>
    /// E.g.
    /// <br/>
    /// "X" -> "context.hand.count"
    /// </summary>
    public required Dictionary<string, ReadExpression> Variables { get; init; }
    public required EffectData[] Effects { get; init; }
}

public record EffectData
{
    public required string Keyword { get; init; }
    public required ReadExpression[] ArgumentExpressions { get; init; }
}

public record TargetData
{
    /// <summary>
    /// E.g.
    /// "context.hand.count > 0"
    /// </summary>
    public required ReadExpression[] ConditionalExpressions { get; init; }
}

/// <summary>
/// <see cref="ReadExpression"/> describes a read-only operation (usually) on the context, or other objects.
/// Should return one of the following types:
/// <br/>
/// - null
/// - int
/// - string
/// - bool
/// - atom
/// - atom[]
/// </summary>
/// <param name="Text"></param>
public record ReadExpression(string Text);