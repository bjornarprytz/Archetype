using System.Collections;
using System.Reflection;
using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.DependencyInjection;
using Archetype.Framework.Design;
using Archetype.Framework.State;

namespace Archetype.Tests.Rules.Inscryption;

public static class Build
{
    public static IRules InscryptionRules()
    {
        var rulesBuilder = new RulesBuilder();
        
        foreach (var keyword in GetKeywords())
        {
            rulesBuilder.AddKeyword(keyword);
        }

        foreach (var phase in GetPhases())
        {
            rulesBuilder.AddPhase(phase);
        }
        
        return rulesBuilder.Build();
    }


    private static IEnumerable<IKeywordDefinition> GetKeywords()
    {
        return GetKeywordsInAssembly(Assembly.GetAssembly(typeof(ChangeZone))!)
            .Concat(
                GetKeywordsInAssembly(Assembly.GetAssembly(typeof(AttackLeshy))!
                    ));
    }
    
    private static IEnumerable<IPhase> GetPhases()
    {
        yield return new UpkeepPhase();
        yield return new MainPhase();
        yield return new CombatPhase();
    }

    private static IEnumerable<IKeywordDefinition> GetKeywordsInAssembly(Assembly assembly)
    {
        foreach (var t in assembly.GetTypes()
                     .Where(t => t.IsSubclassOf(typeof(IKeywordDefinition)) && !t.IsAbstract))
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

            yield return keywordDefinition;
        }
    }
}