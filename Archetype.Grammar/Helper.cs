using Antlr4.Runtime;

namespace Archetype.Grammar;

public static class Helper
{
    public static BaseGrammarParser CreateParser(string code)
    {
        var input = new AntlrInputStream(code);
        var lexer = new BaseGrammarLexer(input);
        var tokens = new CommonTokenStream(lexer);
        return new BaseGrammarParser(tokens);
    }
}