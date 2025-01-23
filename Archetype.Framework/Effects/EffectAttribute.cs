namespace Archetype.Framework.Effects;

[AttributeUsage(AttributeTargets.Method)]
public class EffectAttribute(string keyword) : Attribute
{
    public string Keyword { get; } = keyword;
}

[AttributeUsage(AttributeTargets.Class)]
public class EffectCollectionAttribute : Attribute;