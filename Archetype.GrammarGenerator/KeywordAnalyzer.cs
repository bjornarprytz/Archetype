using System.Reflection;
using System.Text;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Meta;

namespace Archetype.GrammarGenerator;

public class KeywordAnalyzer(string fileName)
{
    public string GenerateKeywordSyntax(Assembly[] targetAssemblies)
    {
        throw new NotImplementedException(); // TODO: Revisit this once the core is implemented
        
        var syntaxTemplate = File.ReadAllText(fileName);

        if (syntaxTemplate == null)
        {
            throw new Exception($"File not found: {fileName}");
        }
        /*
         * 
        var keywordLists = new Dictionary<string, List<KeywordAttribute>?>
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
            var keywordAttribute = keywordType.GetCustomAttribute<KeywordAttribute>()!;
            
            switch (keywordAttribute)
            {
                case TagCollectionAttribute:
                    AppendKeywordSyntax("TAGS_KEYWORD_LIST", keywordAttribute);
                    break;
                case TargetRequirementsAttribute:
                    AppendKeywordSyntax("TARGET_KEYWORD_LIST", keywordAttribute);
                    break;
                case ComputeAttribute:
                    AppendKeywordSyntax("COMPUTED_VALUE_KEYWORD_LIST", keywordAttribute);
                    break;
                case CostAttribute:
                    AppendKeywordSyntax("COST_KEYWORD_LIST", keywordAttribute);
                    break;
                case EffectAttribute:
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
        {*/
         //   var keywordListString = keywords == null ? "'/*N/A*/'"
        //        : "(" + string.Join("|", keywords.Select(SyntaxExtensions.GenerateSyntax)) + ")";
        //    syntaxTemplate = syntaxTemplate!.Replace($"/*{keywordListName}*/", keywordListString);
        //}

        
    }
        /*
         * 
    
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
         
         */

    
}