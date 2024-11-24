namespace Archetype.Framework.State;


[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public class PathPartAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}