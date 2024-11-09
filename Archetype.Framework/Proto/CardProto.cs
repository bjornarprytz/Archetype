using Archetype.Framework.Effects;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework.Core;

public interface ICardProto
{
    public string Name { get; }
    public CostResolver Costs { get; }
    public TargetDescriptor[] Targets { get; }
    
    public Dictionary<string, StatResolver> Stats { get; }
    public Dictionary<string, string[]> Facets { get; }
    public HashSet<string> Tags { get; }
    
    public EffectResolver[] Effects { get; }
    public Dictionary<string, VariableResolver> Variables { get; }
}

public record CardProto : ICardProto
{
    public required string Name { get; init; }
    
    public required CostResolver Costs { get; init; }
    public required TargetDescriptor[] Targets { get; init; }
    
    public required Dictionary<string, StatResolver> Stats { get; init; }
    public required Dictionary<string, string[]> Facets { get; init; }
    public required HashSet<string> Tags { get; init; }
    
    public required EffectResolver[] Effects { get; init; }
    public required Dictionary<string, VariableResolver> Variables { get; init; }
}

public record CostResolver
{
    public Func<IResolutionContext, bool> ValidateCost { get; init; }
    
    public Func<IResolutionContext, IEffectResult> ResolveCost { get; init; }
}

public record StatResolver
{
    public Func<IResolutionContext, int> ResolveStat { get; init; }
}

public record TargetDescriptor
{
    public Func<IResolutionContext, IAtom, bool> ValidateTarget { get; init; }
}

public record VariableResolver
{
    public Func<IResolutionContext, int> ResolveVariable { get; init; }
} 

public record EffectResolver
{
    public Func<IResolutionContext, IEffectResult> ResolveEffect { get; init; }
}