using System;
using System.Linq;
using Archetype.Game.Extensions;
using Archetype.View.Atoms;
using Archetype.View.Infrastructure;
using HotChocolate.Types;
using OneOf;

namespace Archetype.Server.Schema;

public class TargetType : ObjectType<ITargetDescriptor>
{
    protected override void Configure(IObjectTypeDescriptor<ITargetDescriptor> descriptor)
    {
        base.Configure(descriptor);
        
        descriptor.Description("Describes the target of an effect.");
        
        descriptor.Ignore(d => d.TargetType);
    }
}

public class EffectType : ObjectType<IEffectDescriptor>
{
    protected override void Configure(IObjectTypeDescriptor<IEffectDescriptor> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("Describes an effect");
    }
}
public class AffectedType : ObjectType<IAffected>
{
    protected override void Configure(IObjectTypeDescriptor<IAffected> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Ignore(f => f.Description);

        descriptor.Field(d => d.Description.Value)
            .Type<AffectedUnionType>();
    }
    
    public class AffectedUnionType : UnionType
    {
        protected override void Configure(IUnionTypeDescriptor descriptor)
        {
            base.Configure(descriptor);

            descriptor.Type<TargetPropertyType>();
            descriptor.Type<ContextPropertyType>();
        }
    }
}

public class OperandType : ObjectType<IOperand>
{
    protected override void Configure(IObjectTypeDescriptor<IOperand> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Ignore(f => f.Value);
        
        descriptor.Field(f => f.Value.Value)
            .Type<OperandValueUnion>();
        
    }
    
    public class OperandValueUnion : UnionType
    {
        protected override void Configure(IUnionTypeDescriptor descriptor)
        {
            base.Configure(descriptor);

            descriptor.Type<ImmediateOperandValueType>();
            descriptor.Type<TargetPropertyType>();
            descriptor.Type<ContextPropertyType>();
            descriptor.Type<AggregatePropertyType>();
        }
    }
}

public class ImmediateOperandValueType : ObjectType<ImmediateValue>
{
    protected override void Configure(IObjectTypeDescriptor<ImmediateValue> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("Immediate value");
    }
}

public class TargetPropertyType : ObjectType<TargetProperty>
{
    protected override void Configure(IObjectTypeDescriptor<TargetProperty> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("An accessor of a target, or one of its properties");

        descriptor.Ignore(d => d.TargetType);
    }
}

public class ContextPropertyType : ObjectType<ContextProperty>
{
    protected override void Configure(IObjectTypeDescriptor<ContextProperty> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("An accessor of a context property");
    }
}

public class AggregatePropertyType : ObjectType<AggregateProperty>
{
    protected override void Configure(IObjectTypeDescriptor<AggregateProperty> descriptor)
    {
        base.Configure(descriptor);
        
        descriptor.Description("A property that is derived from some aggregate on the context");
    }
}
