namespace Archetype.Framework.Core.Primitives;

public abstract class ComputedValueDefinition : KeywordDefinition
{
    public abstract int Compute(IResolutionContext context, IKeywordInstance keywordInstance);
}