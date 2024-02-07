using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public interface IKeywordDefinition
{
    string Keyword { get; }
}

public interface ICostDefinition : IKeywordDefinition
{
    CostType CostType { get; }
    IEffectResult DryRun(IResolutionContext context, IKeywordInstance keywordInstance, IReadOnlyList<IAtom> payment);
    IEffectResult Pay(IResolutionContext context, IKeywordInstance keywordInstance, IReadOnlyList<IAtom> payment);
}

public interface IEffectDefinition : IKeywordDefinition
{
    IEffectResult Resolve(IResolutionContext context, IKeywordInstance keywordInstance);
}

public interface IComputeDefinition : IKeywordDefinition
{
    int Compute(IResolutionContext context, IKeywordInstance keywordInstance);
}

public interface ITargetDefinition : IKeywordDefinition
{
    IEnumerable<IAtom> GetAllowedTargets(IResolutionContext context, IKeywordInstance keywordInstance);
}