namespace Archetype.Core.Meta;

[AttributeUsage(AttributeTargets.Method)]
public class KeywordAttribute : Attribute
{
    public string Template { get; }

    public KeywordAttribute(string template)
    {
        Template = template;
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
public class DescriptionAttribute : Attribute
{
    public string Description { get; }

    public DescriptionAttribute(string description)
    {
        Description = description;
    }
}