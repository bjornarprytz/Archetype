using Archetype.Framework.Design;
using Archetype.Framework.State;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Framework.DependencyInjection;

public interface IProtoCardBuilderFactory
{
    ProtoCardBuilder CreateBuilder();
}

public class ProtoCardBuilderFactory(IServiceProvider serviceProvider) : IProtoCardBuilderFactory
{
    public ProtoCardBuilder CreateBuilder()
    {
        return serviceProvider.GetRequiredService<ProtoCardBuilder>();
    }
}