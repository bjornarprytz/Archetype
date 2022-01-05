using Archetype.Builder.Builders;
using Archetype.Builder.Factory;
using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Design.Extensions;

public static class ProtoPoolExtensions
{
    public static IProtoPool AddSet(this IProtoPool protoPool, string name, IFactory<ISetBuilder> setBuilderFactory, Action<ISetBuilder> builder)
    {
        var setBuilder = 
            setBuilderFactory
                .Create()
                .Name(name);

        builder(setBuilder);
            
        protoPool.AddSet(setBuilder.Build());

        return protoPool;
    }
}