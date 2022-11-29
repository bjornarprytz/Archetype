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