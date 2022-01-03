using Archetype.View.Proto;
using HotChocolate.Types;

namespace Archetype.Server.Schema;

public abstract class ProtoType<T> : ObjectType<T>
    where T : IProtoDataFront
{
    protected override void Configure(IObjectTypeDescriptor<T> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Field(protoData => protoData.Name);
    }
}

public class CardProtoDataType : ProtoType<ICardProtoDataFront>
{
    protected override void Configure(IObjectTypeDescriptor<ICardProtoDataFront> descriptor)
    {
        base.Configure(descriptor);
       
        descriptor.Description("Blueprint for creating Card instances");
    }
}

public class StructureProtoDataType : ProtoType<IStructureProtoDataFront>
{
    protected override void Configure(IObjectTypeDescriptor<IStructureProtoDataFront> descriptor)
    {
        base.Configure(descriptor);
        descriptor.Description("Blueprint for creating Structure instances");
    }
}

public class CreatureProtoDataType : ProtoType<ICreatureProtoDataFront>
{
    protected override void Configure(IObjectTypeDescriptor<ICreatureProtoDataFront> descriptor)
    {
        descriptor.Description("Blueprint for creating Creature instances");
    }
}