using System.Linq.Expressions;
using Archetype.Components.Extensions;
using Archetype.Components.Meta;
using Archetype.Core.Effects;
using Archetype.Core.Triggers;

namespace Archetype.Components.Builders;

public class TriggerBuilder : ITriggerBuilder
{
    private readonly Trigger _trigger = new ();

    public void PushEffect(Expression<Func<IContext, IResult>> effectFunction)
    {
        _trigger.AddEffect(new Effect(effectFunction));
    }

    public void WithPreCondition(Expression<Func<IContext, bool>> condition)
    {
        // E.g. card needs to be in the hand
        _trigger.SetPreCondition(condition);
    }
    
    public void WhenEventMatches(Expression<Func<IEffectResult, bool>> eventMatchFunction)
    {
        // E.g. keyword==Damage and target==Player
        _trigger.SetEventMatch(eventMatchFunction);
    }

    public ITrigger Build()
    {

        return _trigger;
    }

    private class Trigger : ITrigger
    {
        private Func<IContext, bool> _triggerPreCondition;
        private Func<IEffectResult, bool> _triggerEventMatch;
        private readonly List<IEffectDescriptor> _effectDescriptors;
        private readonly List<Func<IContext, IResult>> _effectFunctions;

        public bool ConditionMet(IContext context, IEffectResult @event)
        {
            return _triggerPreCondition(context) && _triggerEventMatch(@event);
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
        
        public void SetPreCondition(Expression<Func<IContext, bool>> preCondition)
        {
            _triggerPreCondition = preCondition.Compile();
        }
        
        public void SetEventMatch(Expression<Func<IEffectResult, bool>> eventMatch)
        {
            _triggerEventMatch = eventMatch.Compile();
        }

        public void AddEffect(IEffect effect)
        {
            var effectDescriptor = effect.EffectExpression.CreateDescriptor();

            if (effectDescriptor.GetTargets().Any())
            {
                throw new ArgumentException("Trigger effects cannot have targets.");
            }
        
            _effectFunctions.Add(effect.EffectExpression.Compile());
            _effectDescriptors.Add(effectDescriptor);
        }
        
        public string StaticRulesText { get; }
    }
}