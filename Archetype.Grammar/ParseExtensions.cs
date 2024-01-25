using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Archetype.Framework.Core.Primitives;

namespace Archetype.Grammar;

public static class ParseExtensions
{
    public static List<IKeywordInstance> ParseFunctionLike(this IEnumerable<ParserRuleContext> contexts)
    {
        return contexts.Select(ParseFunctionLikeKeyword).Where(k => k != null).ToList()!;
    }
    
    private static IKeywordInstance? ParseFunctionLikeKeyword(this ParserRuleContext keywordContext)
    {
        if (keywordContext.children == null)
        {
            return null;
        }
        
        // TODO: Check for non-existent keywords. Context seems to be just an emtpy string " " when that happens
        
        var operands = keywordContext.ParseOperands().ToArray();
        var keyword = keywordContext.ParseKeyword();
        var keywordInstance = new KeywordInstance(keyword, operands);
        
        return keywordInstance;
    }
    
    internal static string TrimSTRING(this string s)
    {
        return s.Replace("\"", "");
    }
    
    public static IKeywordInstance ParseStaticKeywordContext(this BaseGrammarParser.StaticContext staticKeywordContext)
    {
        var keyword = staticKeywordContext.GetText().Split(":")[0];
        var operands = staticKeywordContext.ParseOperands().ToArray();
        
        if (operands.Length != 1)
        {
            throw new Exception($"Keywords with Key-Value Syntax must have exactly one operand. keyword: {keyword}, operand count: {operands.Length}");
        }
        
        var keywordInstance = new KeywordInstance(keyword, operands);
        
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