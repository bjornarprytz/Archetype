using Archetype.Rules.Proto;

namespace Archetype.Runtime.State;

public interface IActionBlock
{
    public IAtom Source { get; }
    public IReadOnlyList<EffectInstance> Effects { get; }
    public IReadOnlyList<CostInstance> Costs { get; }

    public object? GetComputedValue(string key);
    void UpdateComputedValues(IDefinitions definitions, IGameState gameState);
    
    IEnumerable<TargetDescription> GetTargetDescriptors();
}