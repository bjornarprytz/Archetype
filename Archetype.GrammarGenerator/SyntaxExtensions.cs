using System.Text;
using Antlr4.Runtime.Tree;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.Meta;
using Archetype.Framework.State;

namespace Archetype.GrammarGenerator;

public static class SyntaxExtensions
{
    /*
     * 
    internal static string GenerateSyntax(this KeywordSyntaxAttribute keywordSyntaxAttribute)
    {
        var sb = new StringBuilder();

        switch (keywordSyntaxAttribute)
        {
            case StaticSyntaxAttribute staticSyntaxAttribute:
                sb.Append(staticSyntaxAttribute.KeyValueSyntax());
                break;
            case TargetSyntaxAttribute targetSyntaxAttribute:
                sb.Append(targetSyntaxAttribute.FunctionLikeSyntax());
                break;
            case ConditionSyntaxAttribute conditionSyntaxAttribute:
                sb.Append(conditionSyntaxAttribute.FunctionLikeSyntax());
                break;
            case ComputedValueSyntaxAttribute computedValueSyntaxAttribute:
                sb.Append(computedValueSyntaxAttribute.FunctionLikeSyntax());
                break;
            case EffectSyntaxAttribute effectSyntaxAttribute:
                sb.Append(effectSyntaxAttribute.FunctionLikeSyntax());
                break;
            case CostSyntaxAttribute costSyntaxAttribute:
                sb.Append(costSyntaxAttribute.FunctionLikeSyntax());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(keywordSyntaxAttribute));
            
        }
        
        
        
        return sb.ToString();
    }
    
    private static string KeyValueSyntax(this KeywordSyntaxAttribute keyword)
    {
        if (keyword.Operands.Count != 1)
        {
            throw new Exception($"Keywords with Key-Value Syntax must have exactly one operand. keyword: {keyword.Keyword}, operand count: {keyword.Operands.Count}");
        }
        
        var sb = new StringBuilder();

        sb.Append($"'{keyword.Keyword}:' {keyword.Operands[0].OperandSyntax()}");

        return sb.ToString();
    }
    
    private static string FunctionLikeSyntax(this KeywordAttribute keyword)
    {
        var sb = new StringBuilder();

        sb.Append($"'{keyword.Keyword}");
        
                
        sb.Append('(');
        
        if (keyword.Operands.Count > 0)
        {
            sb.Append('\'');
            sb.Append(string.Join(" ", keyword.Operands.Select(OperandSyntax)));
            sb.Append('\'');
        }
        
        sb.Append(")'");
        
        return sb.ToString();
    }

    private static string OperandSyntax(this IKeywordOperand operandDescription)
    {
        var operandSyntax =  operandDescription.Type == 
                typeof(string) ? "opString" : 
            operandDescription.Type == 
                typeof(int) ? "opNumber" :
            operandDescription.Type.Implements<IAtom>() ? "opAtom" :
                throw new Exception($"Operand type {operandDescription.Type} is not supported");
        
        
        var sb = new StringBuilder(operandSyntax);

        if (operandDescription.IsOptional)
        {
            sb.Append('?');
        }

        return sb.ToString();
    }
     */
}
