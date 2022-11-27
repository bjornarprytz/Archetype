namespace Archetype.Components.Meta;

[AttributeUsage(AttributeTargets.Method)]
internal class KeywordAttribute : Attribute
{
    public string Template { get; }

    public KeywordAttribute(string template)
    {
        Template = template;
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
internal class DescriptionAttribute : Attribute
{
    public string Description { get; }

    public DescriptionAttribute(string description)
    {
        Description = description;
    }
}