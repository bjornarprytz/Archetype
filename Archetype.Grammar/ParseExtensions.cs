using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;

namespace Archetype.Grammar;

public static class ParseExtensions
{
    // TODO: Continue here:
    public static IKeywordOperand ParseOperandContext(this BaseGrammarParser.OperandContext operandContext)
    {
        throw new NotImplementedException();
    }
    
    public static IKeywordInstance ParseEffectContext(this BaseGrammarParser.EffectContext effectContext)
    {
        effectContext.effectKeyword()
        throw new NotImplementedException();
    }
    
    public static IEnumerable<IKeywordInstance> ParseComputedValueContext(this BaseGrammarParser.ComputedValuesContext computedValueContext)
    {
        throw new NotImplementedException();
    }
    
    public static IKeywordInstance ParseConditionContext(this BaseGrammarParser.ConditionKeywordContext conditionKeywordContext)
    {
        throw new NotImplementedException();
    }
    
    public static IKeywordInstance ParseTargetSpecContext(this BaseGrammarParser.TargetSpecsContext targetSpecContext)
    {
        throw new NotImplementedException();
    }
    public static IKeywordInstance ParseCostContext(this BaseGrammarParser.CostKeywordContext costKeywordContext)
    {
        throw new NotImplementedException();
    }
    
    public static IKeywordInstance ParseStaticKeywordContext(this BaseGrammarParser.StaticKeywordContext staticKeywordContext)
    {
        // TOOD: Characteristic syntax (To strive for "TYPE: Creature" instead of "TYPE(Creature)" or Characteristic("TYPE", "Creature"))
        throw new NotImplementedException();
    }
}