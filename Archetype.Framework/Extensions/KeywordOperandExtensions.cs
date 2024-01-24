using Archetype.Framework.Core.Primitives;

namespace Archetype.Framework.Extensions;

public static class KeywordOperandExtensions
{
    public static IKeywordOperand ToOperand(this object? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value is KeywordOperand operand)
        {
            return operand;
        }
        
        return new KeywordOperand(value.GetType(), _ => value);
    }
    
    public static IKeywordOperand<T> ToOperand<T>(this T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value is IKeywordOperand)
        {
            throw new ArgumentException("Cannot convert an operand to an operand");
        }
        
        return new KeywordOperand<T>(value);
    }
}