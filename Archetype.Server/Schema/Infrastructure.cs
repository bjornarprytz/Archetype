using Archetype.View.Infrastructure;
using HotChocolate.Types;

namespace Archetype.Server.Schema;

public class GameStateType : ObjectType<IGameStateFront>
{
    protected override void Configure(IObjectTypeDescriptor<IGameStateFront> descriptor)
    {
        base.Configure(descriptor);
        
        descriptor.Description("The root object of actionable game state");
    }
}

public class MapType : ObjectType<IMapFront>
{
    protected override void Configure(IObjectTypeDescriptor<IMapFront> descriptor)
    {
        base.Configure(descriptor);
        
        descriptor.Description("A graph of map nodes");
    }
}

public class PoolType : ObjectType<IProtoPoolFront>
{
    protected override void Configure(IObjectTypeDescriptor<IProtoPoolFront> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("Collection of all available sets");
    }
}

public class SetType : ObjectType<ISetFront>
{
    protected override void Configure(IObjectTypeDescriptor<ISetFront> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("A set of proto data which share some themes");
    }
}