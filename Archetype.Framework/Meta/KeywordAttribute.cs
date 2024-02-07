using Archetype.Framework.Core.Primitives;

namespace Archetype.Framework.Meta;


[AttributeUsage(AttributeTargets.Method)]
public class ComputeAttribute(string? keyword = null) : KeywordAttribute(keyword);


[AttributeUsage(AttributeTargets.Method)]
public class EffectAttribute(string? keyword = null) : KeywordAttribute(keyword);


[AttributeUsage(AttributeTargets.Method)]
public class TargetRequirementsAttribute(string? keyword = null) : KeywordAttribute(keyword);


[AttributeUsage(AttributeTargets.Method)]
public class CostAttribute(CostType costType, string? keyword = null) : KeywordAttribute(keyword)
{
    public CostType CostType { get; } = costType;
}


public abstract class KeywordAttribute(string? keyword) : Attribute
{
    public string? Keyword { get; } = keyword;
}