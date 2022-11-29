namespace Archetype.Components.Meta;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
internal class DescriptionAttribute : Attribute
{
    public string Description { get; }

    public DescriptionAttribute(string description)
    {
        Description = description;
    }
}