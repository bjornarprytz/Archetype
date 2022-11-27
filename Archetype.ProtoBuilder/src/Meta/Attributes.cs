namespace Archetype.Components.Meta;

internal class PropertyShortHandAttribute : Attribute
{
    public string Path { get; }

    public PropertyShortHandAttribute(string path)
    {
        Path = path;
    }
}
    
internal class DescriptionAttribute : Attribute
{
    public string Description { get; }

    public DescriptionAttribute(string description)
    {
        Description = description;
    }
}