using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public interface IKeywordDefinition
{
    string Keyword { get; }
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