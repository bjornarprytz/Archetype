using System.Reflection;
using Archetype.Framework.DependencyInjection;
using Archetype.Framework.Design;
using Archetype.Framework.Extensions;

namespace Archetype.Prototype1;

public static class BasicRules
{
    public static IRules Create()
    {
        var builder = new RulesBuilder();
        
        builder
            .AddBasicKeywords()
            .AddKeywordsFromAssembly(Assembly.GetExecutingAssembly());
        
        return builder.Build();
    }
}

