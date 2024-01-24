using System.Reflection;
using Archetype.Framework.BaseRules.Keywords;
using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.DependencyInjection;

namespace Archetype.Framework.Extensions;

public static class KeywordExtensions
{
    public static IRulesBuilder AddBasicKeywords(this IRulesBuilder rulesBuilder)
    {
        return rulesBuilder.AddKeywordsFromAssembly(Assembly.GetAssembly(typeof(Effects))!);
    }

    public static IRulesBuilder AddKeywordsFromAssembly(this IRulesBuilder rulesBuilder, Assembly assembly)
    {
        throw new NotImplementedException();
        
        // TODO: Scan for all classes which have methods with the Keyword attribute 

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