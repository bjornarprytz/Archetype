using Archetype.View.Atoms;
using Archetype.View.Infrastructure;
using HotChocolate.Types;

namespace Archetype.Server.Schema;


public class TargetType : ObjectType<ITargetDescriptor>
{
    protected override void Configure(IObjectTypeDescriptor<ITargetDescriptor> descriptor)
    {
        base.Configure(descriptor);
        
        descriptor.Description("Describes the target of an effect.");
    }
}