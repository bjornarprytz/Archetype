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

        descriptor.IsOfType((context, result) => result is ITargetDescriptor);
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

public class ImmediateOperandValueType : ObjectType<IImmediateValue>
{
    protected override void Configure(IObjectTypeDescriptor<IImmediateValue> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("Immediate value");

        descriptor.IsOfType((context, result) => result is IImmediateValue);
    }
}

public class TargetPropertyType : ObjectType<ITargetProperty>
{
    protected override void Configure(IObjectTypeDescriptor<ITargetProperty> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("An accessor of a target, or one of its properties");

        descriptor.Ignore(d => d.TargetType);
        
        descriptor.IsOfType((context, result) => result is ITargetProperty);
    }
}

public class ContextPropertyType : ObjectType<IContextProperty>
{
    protected override void Configure(IObjectTypeDescriptor<IContextProperty> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("An accessor of a context property");
        
        descriptor.IsOfType((context, result) => result is IContextProperty);
    }
}

public class AggregatePropertyType : ObjectType<IAggregateProperty>
{
    protected override void Configure(IObjectTypeDescriptor<IAggregateProperty> descriptor)
    {
        base.Configure(descriptor);
        
        descriptor.Description("A property that is derived from some aggregate on the context");
        
        descriptor.IsOfType((context, result) => result is IAggregateProperty);
    }
}
