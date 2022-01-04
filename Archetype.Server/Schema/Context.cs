using Archetype.View.Atoms;
using Archetype.View.Context;
using Archetype.View.Infrastructure;
using HotChocolate.Types;

namespace Archetype.Server.Schema;


public class TargetType : ObjectType<ITargetFront>
{
    protected override void Configure(IObjectTypeDescriptor<ITargetFront> descriptor)
    {
        base.Configure(descriptor);
        
        descriptor.Description("Describes the target of an effect.");
    }
}

public class TurnContextType : ObjectType<ITurnContext>
{
    protected override void Configure(IObjectTypeDescriptor<ITurnContext> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("Encapsulates a turn.");
        
        //descriptor.Field(c => c.)
    }
}

public class PlayCardContextType : ObjectType<IPlayCardContext>
{
    protected override void Configure(IObjectTypeDescriptor<IPlayCardContext> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("Encapsulates the playing of a card.");
    }
}