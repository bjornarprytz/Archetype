namespace Archetype.Grammar;

public static class TreeTraversalExtensions
{
    public static BaseGrammarParser.TargetSpecsContext[] GetTargetSpecsContext(this BaseGrammarParser.ActionBlockContext actionBlockContext)
    {
        return actionBlockContext?.targets()?.targetSpecs() ?? Array.Empty<BaseGrammarParser.TargetSpecsContext>(); 
    }
    
    public static BaseGrammarParser.ComputedValueContext[] GetComputedValueContext(this BaseGrammarParser.ActionBlockContext actionBlockContext)
    {
        return actionBlockContext?.computedValues()?.computedValue() ?? Array.Empty<BaseGrammarParser.ComputedValueContext>(); 
    }
    
    public static BaseGrammarParser.CostContext[] GetCostContext(this BaseGrammarParser.ActionBlockContext actionBlockContext)
    {
        return actionBlockContext?.cost() ?? Array.Empty<BaseGrammarParser.CostContext>(); 
    }
    
    public static BaseGrammarParser.ConditionContext[] GetConditionContext(this BaseGrammarParser.ActionBlockContext actionBlockContext)
    {
        return actionBlockContext?.condition() ?? Array.Empty<BaseGrammarParser.ConditionContext>(); 
    }
    
    public static BaseGrammarParser.EffectContext[] GetEffectContext(this BaseGrammarParser.ActionBlockContext actionBlockContext)
    {
        return actionBlockContext?.effect() ?? Array.Empty<BaseGrammarParser.EffectContext>(); 
    }
    
    
    public static BaseGrammarParser.AbilityContext[] GetAbilitiesContext(this BaseGrammarParser.CardTextContext cardTextContext)
    {
        return cardTextContext?.abilities()?.ability() ?? Array.Empty<BaseGrammarParser.AbilityContext>();  
    }
}