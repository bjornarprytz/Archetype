using System.Reflection;
using Archetype.BasicRules.Primitives;
using Archetype.Framework.Definitions;
using IDefinitionBuilder = Archetype.Framework.Runtime.IDefinitionBuilder;

namespace Archetype.BasicRules;


public static class Extensions
{
    public static IDefinitionBuilder AddBasicRules(this IDefinitionBuilder definitions)
    {
        foreach (var t in Assembly.GetAssembly(typeof(ChangeZone))!.GetTypes().Where(t => t.IsSubclassOf(typeof(KeywordDefinition)) && !t.IsAbstract))
        {
            if (t == null)
            {
                throw new InvalidOperationException($"Failed to create instance of {t?.FullName}");
            }
            
            var instance = Activator.CreateInstance(t);
            
            if (instance is not KeywordDefinition keywordDefinition)
            {
                throw new InvalidOperationException($"Failed to create instance of {t.FullName}");
            }
            
            Console.WriteLine("Adding keyword: " + keywordDefinition.Name);
            definitions.AddKeyword(keywordDefinition);
        };
        
        return definitions;
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


