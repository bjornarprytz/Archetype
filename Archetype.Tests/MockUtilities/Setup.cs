using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Design;
using Archetype.Framework.Extensions;
using Archetype.Framework.State;
using NSubstitute;

namespace Archetype.Tests.MockUtilities;

public static class Setup
{
    public static IKeywordInstance KeywordInstance(string keyword, params object?[] operands) => KeywordInstance<IKeywordInstance>(keyword, operands);
    public static T KeywordInstance<T>(string keyword, params object?[] operands)
        where T : class, IKeywordInstance
    {
        var keywordInstance = Substitute.For<T>();
        keywordInstance.Keyword.Returns(keyword);
        keywordInstance.Operands.Returns(operands.Select(o => o.ToOperand()).ToArray());
        return keywordInstance;
    }
    
    public static IResolutionFrame ResolutionFrame(IResolutionContext context, params IKeywordInstance[] keywordInstances)
    {
        var resolutionFrame = Substitute.For<IResolutionFrame>();
        resolutionFrame.Context.Returns(context);
        resolutionFrame.Effects.Returns(keywordInstances);
        return resolutionFrame;
    }
    
    public static IEffectDefinition EffectDefinition(string keyword, IEffectResult result)
    {
        var definition = Substitute.For<IEffectDefinition>();
        definition.Keyword.Returns(keyword);
        definition.Resolve(default!, default!).ReturnsForAnyArgs(result);
        return definition;
    }
    
    public static ICostDefinition CostDefinition(string keyword, CostType costType, IEffectResult result)
    {
        var definition = Substitute.For<ICostDefinition>();
        definition.Keyword.Returns(keyword);
        definition.CostType.Returns(costType);
        definition.DryRun(default!, default!, default!).ReturnsForAnyArgs(result);
        definition.Pay(default!, default!, default!).ReturnsForAnyArgs(result);
        return definition;
    }
    
    public static IPaymentContext PaymentContext(IResolutionContext resolutionContext, CostType costType, IKeywordInstance cost, params IAtom[] payments)
    {
        var paymentContext = Substitute.For<IPaymentContext>();
        paymentContext.ResolutionContext.Returns(resolutionContext);
        paymentContext.Costs.Returns(new[] { cost });
        paymentContext.Payments.Returns(new Dictionary<CostType, IReadOnlyList<IAtom>>
        {
            { costType, payments }
        });
        return paymentContext;
    }
    
    public static IPromptContext PromptContext(Guid promptId, params IAtom[] selection)
    {
        var promptContext = Substitute.For<IPromptContext>();
        
        promptContext.PromptId.Returns(promptId);
        promptContext.Selection.Returns(selection);
        
        return promptContext;
    }
}