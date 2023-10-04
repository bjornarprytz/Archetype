using System.Reflection;
using Archetype.BasicRules.Primitives;
using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime;

namespace Archetype.BasicRules;


public static class Extensions
{
    public static IRulesBuilder AddBasicKeywords(this IRulesBuilder ruleses)
    {
        foreach (var t in Assembly.GetAssembly(typeof(ChangeZone))!.GetTypes().Where(t => t.IsSubclassOf(typeof(IKeywordDefinition)) && !t.IsAbstract))
        {
            if (t == null)
            {
                throw new InvalidOperationException($"Failed to create instance of {t?.FullName}");
            }
            
            var instance = Activator.CreateInstance(t);
            
            if (instance is not IKeywordDefinition keywordDefinition)
            {
                throw new InvalidOperationException($"Failed to create instance of {t.FullName}");
            }
            
            Console.WriteLine("Adding keyword: " + keywordDefinition.Name);
            ruleses.AddKeyword(keywordDefinition);
        };
        
        return ruleses;
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


