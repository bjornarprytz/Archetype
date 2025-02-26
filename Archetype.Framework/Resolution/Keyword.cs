using System.Reflection;
using Archetype.Framework.Core;
using Archetype.Framework.Effects;
using Archetype.Framework.Events;
using Archetype.Framework.Parsing;

namespace Archetype.Framework.Resolution;

public interface IKeyword
{
    string Keyword { get; }
    Func<IResolutionContext, IEvent> BindResolver(EffectProto effectProto);
}

internal class KeywordResolver : IKeyword
{
    private readonly MethodInfo _methodInfo;
    
    public KeywordResolver(MethodInfo methodInfo)
    {
        if (methodInfo.GetCustomAttribute<EffectAttribute>() is not { Keyword: { Length: > 0 } keyword })
        {
            throw new ArgumentException("Effect method must have an EffectAttribute with a keyword", nameof(methodInfo));
        }
        
        Keyword = keyword;
        _methodInfo = methodInfo;
    }
    
    public string Keyword { get; private set; }
    

    public Func<IResolutionContext, IEvent> BindResolver(EffectProto effectProto)
    {
        return ctx =>
        {
            var effectParameters = new Queue<IValue>(effectProto.Parameters);

            var parameters = _methodInfo.GetParameters().Select(
                p => p.ParameterType.Implements(typeof(IResolutionContext)) ? 
                    ctx : 
                    effectParameters.Dequeue().GetValue(ctx)).ToArray();
            
            var result = _methodInfo.Invoke(default, parameters);
            
            if (result is IEffectResult effectResult)
            {
                return new Event(effectResult, ctx.GetScope());
            }
            else
            {
                throw new InvalidOperationException("Effect resolver did not return an effect result");
            }

        };
    }
}