using OneOf;

namespace Archetype.Components.Meta;

internal interface IAffected
{
    OneOf<ITargetProperty, IContextProperty> Description { get; }
}
