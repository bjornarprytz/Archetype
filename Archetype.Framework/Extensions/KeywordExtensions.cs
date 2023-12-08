using System.Reflection;
using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.DependencyInjection;

namespace Archetype.Framework.Extensions;

public static class KeywordExtensions
{
    public static IRulesBuilder AddBasicKeywords(this IRulesBuilder rulesBuilder)
    {
        return rulesBuilder.AddKeywordsFromAssembly(Assembly.GetAssembly(typeof(ChangeZone))!);
    }

    public static IRulesBuilder AddKeywordsFromAssembly(this IRulesBuilder rulesBuilder, Assembly assembly)
    {
        foreach (var t in assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(IKeywordDefinition)) && !t.IsAbstract))
        {
            if (t is null)
            {
                throw new InvalidOperationException($"Failed to create instance of {t?.FullName}");
            }
            
            var instance = Activator.CreateInstance(t);
            
            if (instance is not IKeywordDefinition keywordDefinition)
            {
                throw new InvalidOperationException($"Failed to create instance of {t.FullName}");
            }
            
            Console.WriteLine("Adding keyword: " + keywordDefinition.Name);
            rulesBuilder.AddKeyword(keywordDefinition);
        };

        return rulesBuilder;
    }
    
    

    public static void Shuffle<T> (this IList<T> list)
    {
        var random = new Random();
        var n = list.Count;
        
        while (n > 1) 
        {
            n--;
            var k = random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}