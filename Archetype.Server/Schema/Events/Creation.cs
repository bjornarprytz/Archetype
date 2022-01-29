using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;
using Archetype.View.Events;
using HotChocolate.Types;

namespace Archetype.Server.Schema;

public abstract class AtomCreatedEvent<T> : ObjectType<IAtomCreated<T>> where T : IGameAtomFront
{
    protected override void Configure(IObjectTypeDescriptor<IAtomCreated<T>> descriptor)
    {
        base.Configure(descriptor);

        descriptor.IsOfType((context, result) => result is IAtomCreated<T>);
    }
}

public class CreatureCreatedEvent : AtomCreatedEvent<ICreatureFront>
{
    protected override void Configure(IObjectTypeDescriptor<IAtomCreated<ICreatureFront>> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("Occurs when a creature is created");
    }
}

public class StructureCreatedEvent : AtomCreatedEvent<IStructureFront>
{
    protected override void Configure(IObjectTypeDescriptor<IAtomCreated<IStructureFront>> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("Occurs when a structure is created");
    }
}

public class CardCreatedEvent : AtomCreatedEvent<ICardFront>
{
    protected override void Configure(IObjectTypeDescriptor<IAtomCreated<ICardFront>> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("Occurs when a card is created");
    }
}

public class MapNodeCreatedEvent : AtomCreatedEvent<IMapNodeFront>
{
    protected override void Configure(IObjectTypeDescriptor<IAtomCreated<IMapNodeFront>> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("Occurs when a map node is created");
    }
}

public class PlayerCreatedEvent : AtomCreatedEvent<IPlayerFront>
{
    protected override void Configure(IObjectTypeDescriptor<IAtomCreated<IPlayerFront>> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("Occurs when the player is created");
    }
}