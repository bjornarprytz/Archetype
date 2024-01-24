namespace Archetype.Framework.Meta;


[AttributeUsage(AttributeTargets.Method)]
public class ComputeAttribute : KeywordAttribute
{
    public ComputeAttribute(string keyword) : base(keyword)
    {
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class EffectAttribute : KeywordAttribute
{
    public EffectAttribute(string keyword) : base(keyword)
    {
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class TargetRequirementsAttribute : KeywordAttribute
{
    public TargetRequirementsAttribute(string keyword) : base(keyword)
    {
    }
}


public abstract class KeywordAttribute : Attribute
{
    protected KeywordAttribute(string keyword)
    {
        Keyword = keyword;
    }

    public string Keyword { get; }
}