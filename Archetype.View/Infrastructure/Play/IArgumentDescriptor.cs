namespace Archetype.View.Infrastructure;

public interface IArgumentDescriptor
{
    // TODO: This needs to take into consideration the TargetProvider (Extension method context.GetTarget<T>())

    bool NeedsMoreContext { get; }
    string Description { get; }
}