namespace Archetype.Framework.Effects;

[AttributeUsage(AttributeTargets.Method)]
public class EffectAttribute(string _keyword) : Attribute
{
    public string Keyword { get; } = _keyword;
}

[AttributeUsage(AttributeTargets.Class)]
public class EffectCollectionAttribute : Attribute;