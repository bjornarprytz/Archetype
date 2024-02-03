namespace Archetype.Framework.Meta;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class KeywordCollectionAttribute : Attribute {}

public class StatCollectionAttribute : KeywordCollectionAttribute {}
public class TagCollectionAttribute : KeywordCollectionAttribute {}