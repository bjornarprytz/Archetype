using System.Linq.Expressions;
using Archetype.Components.Extensions;
using Archetype.Components.Meta;
using Archetype.Core.Effects;
using Archetype.Core.Triggers;

namespace Archetype.Components.Builders;

public class TriggerBuilder : ITriggerBuilder
{
    private readonly List<Func<IContext, IResult>> _effectFunctions = new ();
    private Func<IContext, IEffectResult, bool>? _triggerCondition;
    private List<IEffectDescriptor> _effectDescriptors = new ();

    public void PushEffect(Expression<Func<IContext, IResult>> effectFunction)
    {
        var effectDescriptor = effectFunction.CreateDescriptor();

        if (effectDescriptor.GetTargets().Any())
        {
            throw new ArgumentException("Trigger effects cannot have targets.");
        }
        
        _effectFunctions.Add(effectFunction.Compile());
        _effectDescriptors.Add(effectDescriptor);
    }

    public void WithCondition(Expression<Func<IContext, IEffectResult, bool>> conditionFunction)
    {
        _triggerCondition = conditionFunction.Compile();
    }

    public ITrigger Build()
    {
        if (_triggerCondition == null)
        {
            throw new InvalidOperationException("Trigger condition is not set.");
        }

        if (!_effectFunctions.Any())
        {
            throw new InvalidOperationException("Trigger effects must be non-empty.");
        }
        
        return new Trigger(_triggerCondition, _effectFunctions, _effectDescriptors);
    }

    private class Trigger : ITrigger
    {
        private readonly Func<IContext, IEffectResult, bool> _triggerCondition;
        private readonly List<IEffectDescriptor> _effectDescriptors;
        private readonly List<Func<IContext, IResult>> _effectFunctions;

        public Trigger(Func<IContext, IEffectResult, bool> triggerCondition, IEnumerable<Func<IContext, IResult>> effectFunctions, IEnumerable<IEffectDescriptor> effectDescriptors)
        {
            _triggerCondition = triggerCondition;
            _effectFunctions = new (effectFunctions);
            _effectDescriptors = new (effectDescriptors);
        }


        public bool ConditionMet(IContext context, IEffectResult @event)
        {
            return _triggerCondition(context, @event);
        }

        public IResult Resolve(IContext context)
        {
            return IResult.Join(_effectFunctions.Select(effect => effect(context)));
        }

        public string ContextualRulesText(IContext context)
        {
            throw new NotImplementedException();
            
            // Use the condition in the text
            // Use the effect descriptors to generate a string that describes the trigger's effects.
        }

        public string StaticRulesText { get; }
    }
}