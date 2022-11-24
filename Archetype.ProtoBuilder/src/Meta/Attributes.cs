namespace Archetype.Components.Meta;

internal class PropertyShortHandAttribute : Attribute
{
    public string Path { get; }

    public PropertyShortHandAttribute(string path)
    {
        Path = path;
    }
}
    
internal class ContextFactAttribute : Attribute
{
    public string Description { get; }

    public ContextFactAttribute(string description)
    {
        Description = description;
    }
}