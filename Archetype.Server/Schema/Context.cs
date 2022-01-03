using Archetype.View.Infrastructure;
using HotChocolate.Types;

namespace Archetype.Server.Schema;

public class TargetType : ObjectType<ITargetFront>
{
    protected override void Configure(IObjectTypeDescriptor<ITargetFront> descriptor)
    {
        base.Configure(descriptor);
        
        descriptor.Description("The target of a card");
    }
}