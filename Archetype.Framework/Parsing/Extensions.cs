using Archetype.Framework.Proto;

namespace Archetype.Framework.Parsing;

public static class Extensions
{
    public static KeywordOperand ToOperand(this ActionBlockParser.OperandContext context)
    {
        var text = context.GetText();
        // TODO: Contiune here
        
        return new KeywordOperand
        {
            
        };
    }
}