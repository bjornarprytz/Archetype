namespace Archetype.Framework.Meta;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class KeywordAttribute(string keyword) : Attribute
{
    public string Keyword => keyword;
}