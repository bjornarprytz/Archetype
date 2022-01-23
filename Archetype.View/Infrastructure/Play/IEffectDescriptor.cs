using Archetype.View.Extensions;
using OneOf;

namespace Archetype.View.Infrastructure;

public interface IEffectDescriptor
{
    
    IAffected Affected { get; }
    string Keyword { get; }
    IEnumerable<IOperand> Operands { get; }
}

public interface IAffected
{
    OneOf<ITargetProperty, IContextProperty> Description { get; }
}

public record EffectDescriptor(IAffected Affected, string Keyword, IEnumerable<IOperand> Operands) : IEffectDescriptor;



public record Affected : IAffected 
{
    public Affected(IOneOf oneOf)
    {
        Description = oneOf.Value switch
        {
            ITargetProperty tp => OneOf<ITargetProperty, IContextProperty>
                .FromT0(tp),
            IContextProperty cp => OneOf<ITargetProperty, IContextProperty>
                .FromT1(cp),
            _ => throw new ArgumentException($"Cannot parse affected of value type {oneOf.Value} "),
        };
    }
    
    public Affected(ITargetProperty description)
    {
        Description = OneOf<ITargetProperty, IContextProperty>.FromT0(description);
    }
    
    public Affected(IContextProperty description)
    {
        Description = OneOf<ITargetProperty, IContextProperty>.FromT1(description);
    }
    
    public OneOf<ITargetProperty, IContextProperty> Description { get; }
}