using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public class CostDefinition : ICostDefinition
{
    public string Keyword { get; }
    public CostType CostType { get; }
    public IEffectResult DryRun(IResolutionContext context, IKeywordInstance keywordInstance, IReadOnlyList<IAtom> payment)
    {
        throw new NotImplementedException();
    }

    public IEffectResult Pay(IResolutionContext context, IKeywordInstance keywordInstance, IReadOnlyList<IAtom> payment)
    {
        throw new NotImplementedException();
    }
}