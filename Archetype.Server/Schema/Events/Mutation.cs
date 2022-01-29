using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;
using Archetype.View.Events;
using HotChocolate.Types;

namespace Archetype.Server.Schema;


public abstract class AtomMutationEvent<T> : ObjectType<IAtomMutation<T>>
    where T : IGameAtomFront
{
    protected override void Configure(IObjectTypeDescriptor<IAtomMutation<T>> descriptor)
    {
        base.Configure(descriptor);

        descriptor.IsOfType((context, result) => result is IAtomMutation<T>);
    }
}

public class CreatureMutationEvent : AtomMutationEvent<ICreatureFront>
{
    protected override void Configure(IObjectTypeDescriptor<IAtomMutation<ICreatureFront>> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("Occurs when a creature changes");
    }
}

public class StructureMutationEvent : AtomMutationEvent<IStructureFront>
{
    protected override void Configure(IObjectTypeDescriptor<IAtomMutation<IStructureFront>> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("Occurs when a structure changes");
    }
}

public class CardMutationEvent : AtomMutationEvent<ICardFront>
{
    protected override void Configure(IObjectTypeDescriptor<IAtomMutation<ICardFront>> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("Occurs when a card changes");
    }
}

public class MapNodeMutationEvent : AtomMutationEvent<IMapNodeFront>
{
    protected override void Configure(IObjectTypeDescriptor<IAtomMutation<IMapNodeFront>> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("Occurs when a map node changes");
    }
}

public class PlayerMutationEvent : AtomMutationEvent<IPlayerFront>
{
    protected override void Configure(IObjectTypeDescriptor<IAtomMutation<IPlayerFront>> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("Occurs when the player changes");
    }
}