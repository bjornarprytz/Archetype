using HotChocolate.Types;

namespace Archetype.Server.Schema;

public class PlayCardEvent : ObjectType<Mutations.PlayCardPayload>
{
    protected override void Configure(IObjectTypeDescriptor<Mutations.PlayCardPayload> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Field(payload => payload.Events)
            .Type<ListType<AtomMutationEventUnion>>();
    }
    
    public class AtomMutationEventUnion : UnionType
    {
        protected override void Configure(IUnionTypeDescriptor descriptor)
        {
            base.Configure(descriptor);

            descriptor.Type<CreatureMutationEvent>();
            descriptor.Type<StructureMutationEvent>();
            descriptor.Type<CardMutationEvent>();
            descriptor.Type<MapNodeMutationEvent>();
            descriptor.Type<PlayerMutationEvent>();
        }
    }
    
}