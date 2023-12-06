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
        // Use the DI container to create an instance of ProtoCardBuilder
        return serviceProvider.GetRequiredService<ProtoCardBuilder>();
    }
}