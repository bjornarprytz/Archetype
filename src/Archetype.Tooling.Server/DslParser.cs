using Archetype.Core;

namespace Archetype.Tooling.Server;

// ---------------------------------------------------------------------------
//  DslParser — converts DSL source text to KeywordNode trees (D2, D27)
// ---------------------------------------------------------------------------
//
//  DSL grammar (subset supported by this parser):
//
//    expression   = invocation | literal | paramref
//    invocation   = name '(' args? ')'
//    args         = expression (',' expression)*
//    paramref     = identifier          (a bare identifier not followed by '(')
//    literal      = NUMBER | STRING | BOOL
//    name         = identifier          (followed by '(')
//    identifier   = [a-zA-Z_][a-zA-Z0-9_-]*
//    NUMBER       = [-]? [0-9]+ ('.' [0-9]+)?
//    STRING       = '"' [^"]* '"'
//    BOOL         = 'true' | 'false'
//
//  The parser is hand-written recursive descent.  It is tolerant of partial
//  input so that GetCompletions can call it with an incomplete DSL string.
//
//  On parse error, ParseResult.Error is set and Node is null.

/// <summary>
/// Parses a DSL source string into a <see cref="KeywordNode"/> expression tree.
/// </summary>
public static class DslParser
{
    // -----------------------------------------------------------------------
    //  Public entry points
    // -----------------------------------------------------------------------

    /// <summary>
    /// Attempts to parse a complete DSL expression.
    /// Returns a <see cref="ParseResult"/> — check <see cref="ParseResult.IsSuccess"/>
    /// before using <see cref="ParseResult.Node"/>.
    /// </summary>
    public static ParseResult Parse(string dsl)
    {
        if (string.IsNullOrWhiteSpace(dsl))
            return ParseResult.Fail(0, "Empty DSL expression.");

        var tokenizer = new Tokenizer(dsl);
        var tokens    = tokenizer.Tokenize();
        var parser    = new Parser(tokens);
        return parser.ParseExpression();
    }

    /// <summary>
    /// Parses an effect block — a semicolon-separated list of DSL expressions,
    /// each mapping to one <see cref="EffectBlockStep"/>.
    /// </summary>
    public static BlockParseResult ParseBlock(string dsl)
    {
        if (string.IsNullOrWhiteSpace(dsl))
            return new BlockParseResult(new EffectBlockDef([]), []);

        // Split on ';' respecting parenthesis nesting.
        var statements  = SplitStatements(dsl);
        var steps       = new List<EffectBlockStep>(statements.Count);
        var diagnostics = new List<ParseDiagnostic>();
        int offset      = 0;

        foreach (var stmt in statements)
        {
            var trimmed = stmt.Trim();
            if (trimmed.Length == 0) { offset += stmt.Length + 1; continue; }

            var result = Parse(trimmed);
            if (result.IsSuccess)
            {
                // A block step must be a top-level Invocation.
                if (result.Node is Invocation inv)
                {
                    steps.Add(new EffectBlockStep(inv.KeywordName, inv.Args));
                }
                else
                {
                    diagnostics.Add(new ParseDiagnostic(
                        offset,
                        offset + trimmed.Length,
                        "Effect block step must be a keyword invocation."));
                }
            }
            else
            {
                diagnostics.Add(new ParseDiagnostic(
                    offset + (result.ErrorOffset ?? 0),
                    offset + trimmed.Length,
                    result.ErrorMessage ?? "Parse error."));
            }
            offset += stmt.Length + 1; // +1 for the ';'
        }

        return new BlockParseResult(new EffectBlockDef(steps), diagnostics);
    }

    // -----------------------------------------------------------------------
    //  Statement splitter — splits on ';' outside parentheses
    // -----------------------------------------------------------------------

    private static List<string> SplitStatements(string dsl)
    {
        var parts = new List<string>();
        int depth = 0;
        int start = 0;
        for (int i = 0; i < dsl.Length; i++)
        {
            char c = dsl[i];
            if (c == '(') depth++;
            else if (c == ')') depth--;
            else if (c == ';' && depth == 0)
            {
                parts.Add(dsl[start..i]);
                start = i + 1;
            }
        }
        parts.Add(dsl[start..]);
        return parts;
    }

    // -----------------------------------------------------------------------
    //  Token types
    // -----------------------------------------------------------------------

    private enum TokenKind
    {
        Identifier, // bare word or keyword name
        Number,
        StringLit,
        Bool,
        LeftParen,
        RightParen,
        Comma,
        Eof,
    }

    private readonly record struct Token(TokenKind Kind, string Text, int Offset);

    // -----------------------------------------------------------------------
    //  Tokenizer
    // -----------------------------------------------------------------------

    private sealed class Tokenizer(string source)
    {
        private int _pos;

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();
            while (_pos < source.Length)
            {
                SkipWhitespace();
                if (_pos >= source.Length) break;

                char c = source[_pos];

                if (c == '(') { tokens.Add(new Token(TokenKind.LeftParen,  "(", _pos)); _pos++; }
                else if (c == ')') { tokens.Add(new Token(TokenKind.RightParen, ")", _pos)); _pos++; }
                else if (c == ',') { tokens.Add(new Token(TokenKind.Comma, ",", _pos)); _pos++; }
                else if (c == '"') tokens.Add(ReadString());
                else if (c == '-' || char.IsDigit(c)) tokens.Add(ReadNumber());
                else if (char.IsLetter(c) || c == '_') tokens.Add(ReadIdentifier());
                else _pos++; // skip unknown character
            }
            tokens.Add(new Token(TokenKind.Eof, "", source.Length));
            return tokens;
        }

        private void SkipWhitespace()
        {
            while (_pos < source.Length && char.IsWhiteSpace(source[_pos]))
                _pos++;
        }

        private Token ReadString()
        {
            int start = _pos++;
            while (_pos < source.Length && source[_pos] != '"')
            {
                if (source[_pos] == '\\') _pos++; // skip escaped char
                _pos++;
            }
            if (_pos < source.Length) _pos++; // consume closing "
            return new Token(TokenKind.StringLit, source[start.._pos], start);
        }

        private Token ReadNumber()
        {
            int start = _pos;
            if (_pos < source.Length && source[_pos] == '-') _pos++;
            while (_pos < source.Length && char.IsDigit(source[_pos])) _pos++;
            if (_pos < source.Length && source[_pos] == '.')
            {
                _pos++;
                while (_pos < source.Length && char.IsDigit(source[_pos])) _pos++;
            }
            // If it turned out to be just '-', treat as identifier
            string text = source[start.._pos];
            if (text == "-") return new Token(TokenKind.Identifier, text, start);
            return new Token(TokenKind.Number, text, start);
        }

        private Token ReadIdentifier()
        {
            int start = _pos;
            // Identifiers may include hyphens (e.g. modify-accumulator) but not at start/end.
            // We read until whitespace or a syntax character.
            while (_pos < source.Length)
            {
                char c = source[_pos];
                if (char.IsLetterOrDigit(c) || c == '_' || c == '-') _pos++;
                else break;
            }
            string text = source[start.._pos];
            if (text.Equals("true",  StringComparison.OrdinalIgnoreCase))
                return new Token(TokenKind.Bool, text, start);
            if (text.Equals("false", StringComparison.OrdinalIgnoreCase))
                return new Token(TokenKind.Bool, text, start);
            return new Token(TokenKind.Identifier, text, start);
        }
    }

    // -----------------------------------------------------------------------
    //  Recursive-descent parser
    // -----------------------------------------------------------------------

    private sealed class Parser(List<Token> tokens)
    {
        private int _pos;

        private Token Current => _pos < tokens.Count ? tokens[_pos] : tokens[^1];
        private Token Peek(int ahead = 1) =>
            (_pos + ahead) < tokens.Count ? tokens[_pos + ahead] : tokens[^1];

        public ParseResult ParseExpression()
        {
            if (Current.Kind == TokenKind.Eof)
                return ParseResult.Fail(Current.Offset, "Unexpected end of expression.");

            // Invocation: identifier followed by '('
            if (Current.Kind == TokenKind.Identifier && Peek().Kind == TokenKind.LeftParen)
                return ParseInvocation();

            // Boolean literal
            if (Current.Kind == TokenKind.Bool)
            {
                bool val = Current.Text.Equals("true", StringComparison.OrdinalIgnoreCase);
                _pos++;
                return ParseResult.Ok(new Literal(val));
            }

            // Number literal
            if (Current.Kind == TokenKind.Number)
            {
                if (!double.TryParse(Current.Text,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out double d))
                    return ParseResult.Fail(Current.Offset, $"Invalid number '{Current.Text}'.");
                _pos++;
                return ParseResult.Ok(new Literal(d));
            }

            // String literal
            if (Current.Kind == TokenKind.StringLit)
            {
                // Strip surrounding quotes and unescape
                string raw = Current.Text;
                string value = raw.Length >= 2
                    ? raw[1..^1].Replace("\\\"", "\"").Replace("\\\\", "\\")
                    : "";
                _pos++;
                return ParseResult.Ok(new Literal(value));
            }

            // Parameter reference: bare identifier not followed by '('
            if (Current.Kind == TokenKind.Identifier)
            {
                string name = Current.Text;
                _pos++;
                return ParseResult.Ok(new ParameterRef(name));
            }

            return ParseResult.Fail(Current.Offset,
                $"Unexpected token '{Current.Text}' ({Current.Kind}).");
        }

        private ParseResult ParseInvocation()
        {
            string name = Current.Text;
            int nameOffset = Current.Offset;
            _pos++; // consume identifier

            if (Current.Kind != TokenKind.LeftParen)
                return ParseResult.Fail(Current.Offset, $"Expected '(' after '{name}'.");
            _pos++; // consume '('

            var args = new List<KeywordNode>();

            // Empty argument list
            if (Current.Kind == TokenKind.RightParen)
            {
                _pos++;
                return ParseResult.Ok(new Invocation(name, [.. args]));
            }

            // Parse comma-separated arguments
            while (true)
            {
                var argResult = ParseExpression();
                if (!argResult.IsSuccess)
                    return argResult; // propagate error with original offset

                args.Add(argResult.Node!);

                if (Current.Kind == TokenKind.Comma)
                {
                    _pos++; // consume ','
                    continue;
                }

                if (Current.Kind == TokenKind.RightParen)
                {
                    _pos++; // consume ')'
                    return ParseResult.Ok(new Invocation(name, [.. args]));
                }

                // Tolerate missing ')' at EOF (partial input for completion)
                if (Current.Kind == TokenKind.Eof)
                    return ParseResult.Ok(new Invocation(name, [.. args]));

                return ParseResult.Fail(Current.Offset,
                    $"Expected ',' or ')' but found '{Current.Text}'.");
            }
        }
    }
}

// ---------------------------------------------------------------------------
//  Parse result types
// ---------------------------------------------------------------------------

/// <summary>
/// Result of parsing a single DSL expression.
/// </summary>
public sealed record ParseResult
{
    /// <summary>The parsed node; non-null when <see cref="IsSuccess"/> is true.</summary>
    public KeywordNode? Node { get; init; }
    /// <summary>True when parsing succeeded.</summary>
    public bool IsSuccess { get; init; }
    /// <summary>Human-readable error message; non-null when <see cref="IsSuccess"/> is false.</summary>
    public string? ErrorMessage { get; init; }
    /// <summary>Byte offset into the source string where the error occurred.</summary>
    public int? ErrorOffset { get; init; }

    /// <summary>Constructs a success result.</summary>
    public static ParseResult Ok(KeywordNode node) =>
        new() { Node = node, IsSuccess = true };

    /// <summary>Constructs a failure result.</summary>
    public static ParseResult Fail(int offset, string message) =>
        new() { IsSuccess = false, ErrorOffset = offset, ErrorMessage = message };
}

/// <summary>
/// Result of parsing a full effect block (semicolon-separated statements).
/// </summary>
/// <param name="Block">The parsed block; steps with parse errors are omitted.</param>
/// <param name="Diagnostics">All parse errors, with source offsets into the original DSL string.</param>
public sealed record BlockParseResult(
    EffectBlockDef Block,
    IReadOnlyList<ParseDiagnostic> Diagnostics)
{
    /// <summary>True when all statements parsed without error.</summary>
    public bool IsSuccess => Diagnostics.Count == 0;
}

/// <summary>A parse-time diagnostic with source position.</summary>
public sealed record ParseDiagnostic(int Start, int End, string Message);
