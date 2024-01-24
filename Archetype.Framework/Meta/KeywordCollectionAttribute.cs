namespace Archetype.Framework.Meta;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class KeywordCollectionAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ComputeCollectionAttribute : Attribute{}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class StatCollectionAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TagCollectionAttribute : Attribute { }