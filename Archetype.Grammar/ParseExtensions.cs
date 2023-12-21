using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;

namespace Archetype.Grammar;

public static class ParseExtensions
{
    public static IKeywordInstance ParseEffectContext(this BaseGrammarParser.EffectContext effectContext)
    {
        // TODO: Check for non-existent keywords. Context seems to be just an emtpy string " " when that happens
        
        var operands = effectContext.ParseOperands().ToList();
        var effectKeyword = effectContext.ParseKeyword();
        var keywordInstance = new KeywordInstance
        {
            Keyword = effectKeyword,
            Operands = operands,
        };
        
        return keywordInstance;
    }
    
    internal static string TrimSTRING(this string s)
    {
        return s.Replace("\"", "");
    }
    
    public static IEnumerable<IKeywordInstance> ParseComputedValueContext(this BaseGrammarParser.ComputedValuesContext? computedValueContext)
    {
        if (computedValueContext is null)
        {
            return Enumerable.Empty<IKeywordInstance>();
        }
        
        throw new NotImplementedException();
    }
    
    public static IKeywordInstance ParseConditionContext(this BaseGrammarParser.ConditionContext conditionKeywordContext)
    {
        throw new NotImplementedException();
    }
    
    public static IKeywordInstance ParseTargetSpecContext(this BaseGrammarParser.TargetSpecsContext targetSpecContext)
    {
        var operands = targetSpecContext.ParseOperands().ToList();
        var keyword = targetSpecContext.ParseKeyword();
        var keywordInstance = new KeywordInstance
        {
            Keyword = keyword,
            Operands = operands,
        };
        
        return keywordInstance;
    }
    public static IKeywordInstance ParseCostContext(this BaseGrammarParser.CostContext costKeywordContext)
    {
        var operands = costKeywordContext.ParseOperands().ToList();
        var keyword = costKeywordContext.ParseKeyword();
        var keywordInstance = new KeywordInstance
        {
            Keyword = keyword,
            Operands = operands,
        };
        
        return keywordInstance;
    }
    
    public static IKeywordInstance ParseStaticKeywordContext(this BaseGrammarParser.StaticContext staticKeywordContext)
    {
        var keyword = staticKeywordContext.GetText().Split(":")[0];
        var operands = staticKeywordContext.ParseOperands().ToList();
        
        if (operands.Count != 1)
        {
            throw new Exception($"Keywords with Key-Value Syntax must have exactly one operand. keyword: {keyword}, operand count: {operands.Count}");
        }
        
        var keywordInstance = new KeywordInstance
        {
            Keyword = keyword,
            Operands = operands,
        };
        
        return keywordInstance;
    }

    private static IEnumerable<IKeywordOperand> ParseOperands(this ParserRuleContext context)
    {
        return context.children == null ? Enumerable.Empty<IKeywordOperand>() 
            : context.children.Select(Parse).Where(k => k != null).ToList()!;

        IKeywordOperand? Parse(IParseTree tree)
        {
            return tree switch
            {
                BaseGrammarParser.OpAtomContext atom => ParseAtomOperand(atom),
                BaseGrammarParser.OpNumberContext number => ParseNumberOperand(number) ,
                BaseGrammarParser.OpStringContext @string => ParseStringOperand(@string),
                _ => null
            };
        }

        IKeywordOperand ParseAtomOperand(BaseGrammarParser.OpAtomContext atom)
        {
            if (atom.targetRef() is { } targetRef)
            {
                return new TargetRef(int.Parse(targetRef.index().GetText()));
            }

            if (atom.SELF() is { } self)
            {
                return new TargetSourceRef();
            }
            throw new Exception($"Unknown atom operand type encountered: {{atom.GetText()}}");
        }
        
        IKeywordOperand ParseNumberOperand(BaseGrammarParser.OpNumberContext number)
        {
            if (number.NUMBER() is {} numberToken)
            {
                return new KeywordOperand<int>(int.Parse(numberToken.GetText()));
            }

            if (number.computedValueRef() is {} computedValueRef)
            {
                return new ComputeRef(int.Parse(computedValueRef.index().GetText()));
            }
            throw new Exception($"Unknown number operand type encountered: {{number.GetText()}}");
        }
        
        IKeywordOperand ParseStringOperand(BaseGrammarParser.OpStringContext @string)
        {
            return new KeywordOperand<string>(@string.STRING().GetText().TrimSTRING());
        }
    }

    private static string ParseKeyword(this ParserRuleContext context)
    {
        // TODO: Regex this to remove the need for the trim
        return context.children.First().GetText().Trim(')', '(', ':').Trim();
    }
}