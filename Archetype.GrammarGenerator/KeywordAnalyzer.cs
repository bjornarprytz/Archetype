using System.Reflection;
using System.Text;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Meta;

namespace Archetype.GrammarGenerator;

public class KeywordAnalyzer(string fileName)
{
    public string GenerateKeywordSyntax(Assembly[] targetAssemblies)
    {
        var syntaxTemplate = File.ReadAllText(fileName);

        if (syntaxTemplate == null)
        {
            throw new Exception($"File not found: {fileName}");
        }
        
        // TODO: Generate these lists from the keyword classes
        /*STATIC_KEYWORD_LIST*/
        /*TARGET_KEYWORD_LIST*/;
        /*CONDITION_KEYWORD_LIST*/;
        /*COMPUTED_VALUE_KEYWORD_LIST*/;
        /*COST_KEYWORD_LIST*/;
        /*EFFECT_KEYWORD_LIST*/;
        
        var keywordLists = new Dictionary<string, List<KeywordSyntaxAttribute>?>
        {
            {"STATIC_KEYWORD_LIST", null},
            {"TARGET_KEYWORD_LIST", null},
            {"CONDITION_KEYWORD_LIST", null},
            {"COMPUTED_VALUE_KEYWORD_LIST", null},
            {"COST_KEYWORD_LIST", null},
            {"EFFECT_KEYWORD_LIST", null}
        };

        foreach (var keywordType in GetKeywords(targetAssemblies))
        {
            var keywordAttribute = keywordType.GetCustomAttribute<KeywordSyntaxAttribute>()!;
            
            switch (keywordAttribute)
            {
                case StaticSyntaxAttribute:
                    AppendKeywordSyntax("STATIC_KEYWORD_LIST", keywordAttribute);
                    break;
                case TargetSyntaxAttribute:
                    AppendKeywordSyntax("TARGET_KEYWORD_LIST", keywordAttribute);
                    break;
                case ConditionSyntaxAttribute:
                    AppendKeywordSyntax("CONDITION_KEYWORD_LIST", keywordAttribute);
                    break;
                case ComputedValueSyntaxAttribute:
                    AppendKeywordSyntax("COMPUTED_VALUE_KEYWORD_LIST", keywordAttribute);
                    break;
                case CostSyntaxAttribute:
                    AppendKeywordSyntax("COST_KEYWORD_LIST", keywordAttribute);
                    break;
                case EffectSyntaxAttribute:
                    AppendKeywordSyntax("EFFECT_KEYWORD_LIST", keywordAttribute);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        foreach (var (keywordListName, keywords) in keywordLists)
        {
            ReplaceKeywordList(keywordListName, keywords);
        }

        return syntaxTemplate;

        void AppendKeywordSyntax(string keywordListName, KeywordSyntaxAttribute keyword)
        {
            (keywordLists[keywordListName] ??= new List<KeywordSyntaxAttribute>()).Add(keyword);
        }

        void ReplaceKeywordList(string keywordListName, IEnumerable<KeywordSyntaxAttribute>? keywords)
        {
            var keywordListString = keywords == null ? "'NOTHING'" // TODO: Make something more elegant for non-existent lists
                : "(" + string.Join("|", keywords.Select(ExtractSyntax)) + ")";
            syntaxTemplate = syntaxTemplate!.Replace($"/*{keywordListName}*/", keywordListString);
        }
    }
    
    
    private static IEnumerable<Type> GetKeywords(IEnumerable<Assembly> assemblies)
    {
        var keywordClasses = new List<Type>();
        foreach (var classesWithKeywordAttribute in assemblies.Select(assembly => assembly.GetTypes()
                     .Where(type => type.GetCustomAttribute<KeywordSyntaxAttribute>() != null)))
        {
            keywordClasses.AddRange(classesWithKeywordAttribute);
        }

        return keywordClasses
            .Where(type => !string.IsNullOrWhiteSpace(type.GetCustomAttribute<KeywordSyntaxAttribute>()?.Keyword))
            .ToArray();
    }

    private static string ExtractSyntax(KeywordSyntaxAttribute keywordSyntaxAttribute)
    {
        var sb = new StringBuilder();

        sb.Append($"'{keywordSyntaxAttribute.Keyword}");
        
                
        sb.Append('(');
        
        if (keywordSyntaxAttribute.Operands.Count > 0)
        {
            sb.Append('\'');
            sb.Append(string.Join(" ", keywordSyntaxAttribute.Operands.Select(ExtractSyntax)));
            sb.Append('\'');
            sb.Append("YO");
        }
        
        sb.Append(")'");
        
        
        return sb.ToString();
    }

    private static string ExtractSyntax(IOperandDescription operandDescription)
    {
        var sb = new StringBuilder("operand"); // TODO: Make the type matter here, first we need syntax for the types

        if (operandDescription.IsOptional)
        {
            sb.Append('?');
        }

        return sb.ToString();
    }
}