namespace Archetype.Framework.Core.Primitives;

public abstract class ConditionDefinition : KeywordDefinition
{
    public abstract bool Check(IResolutionContext context, IKeywordInstance keywordInstance); 
}