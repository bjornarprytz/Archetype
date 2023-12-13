using System.Reflection;
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
        
        var keywordLists = new Dictionary<string, IEnumerable<string>>
        {
            {"STATIC_KEYWORD_LIST", new string[]{}},
            {"TARGET_KEYWORD_LIST", new string[]{}},
            {"CONDITION_KEYWORD_LIST", new string[]{}},
            {"COMPUTED_VALUE_KEYWORD_LIST", new string[]{}},
            {"COST_KEYWORD_LIST", new string[]{}},
            {"EFFECT_KEYWORD_LIST", new string[]{}}
        };

        foreach (var keywordType in GetKeywords(targetAssemblies))
        {
            var keywordAttribute = keywordType.GetCustomAttribute<KeywordAttribute>()!;
            
            switch (keywordAttribute)
            {
                case StaticKeywordAttribute:
                    keywordLists["STATIC_KEYWORD_LIST"] = keywordLists["STATIC_KEYWORD_LIST"].Append(keywordAttribute.Keyword);
                    break;
                case TargetKeywordAttribute:
                    keywordLists["TARGET_KEYWORD_LIST"] = keywordLists["TARGET_KEYWORD_LIST"].Append(keywordAttribute.Keyword);
                    break;
                case ConditionKeywordAttribute:
                    keywordLists["CONDITION_KEYWORD_LIST"] = keywordLists["CONDITION_KEYWORD_LIST"].Append(keywordAttribute.Keyword);
                    break;
                case ComputedValueKeywordAttribute:
                    keywordLists["COMPUTED_VALUE_KEYWORD_LIST"] = keywordLists["COMPUTED_VALUE_KEYWORD_LIST"].Append(keywordAttribute.Keyword);
                    break;
                case CostKeywordAttribute:
                    keywordLists["COST_KEYWORD_LIST"] = keywordLists["COST_KEYWORD_LIST"].Append(keywordAttribute.Keyword);
                    break;
                case EffectKeywordAttribute:
                    keywordLists["EFFECT_KEYWORD_LIST"] = keywordLists["EFFECT_KEYWORD_LIST"].Append(keywordAttribute.Keyword);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        foreach (var (keywordListName, keywords) in keywordLists)
        {
            replaceKeywordList(keywordListName, keywords);
        }
        
        
        
        void replaceKeywordList(string keywordListName, IEnumerable<string> keywords)
        {
            var keywordListString = "(" + string.Join("|", keywords) + ")";
            syntaxTemplate = syntaxTemplate!.Replace($"/*{keywordListName}*/", keywordListString);
        }
        
    }
    
    
    private static IEnumerable<Type> GetKeywords(IEnumerable<Assembly> assemblies)
    {
        var keywordClasses = new List<Type>();
        foreach (var classesWithKeywordAttribute in assemblies.Select(assembly => assembly.GetTypes()
                     .Where(type => type.GetCustomAttribute<KeywordAttribute>() != null)))
        {
            keywordClasses.AddRange(classesWithKeywordAttribute);
        }

        return keywordClasses
            .Where(type => !string.IsNullOrWhiteSpace(type.GetCustomAttribute<KeywordAttribute>()?.Keyword))
            .ToArray();
    }
}