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
        // Use the DI container to create an instance of ProtoCardBuilder
        return serviceProvider.GetRequiredService<ProtoCardBuilder>();
    }
}

public interface IPhaseFactory
{
    public TPhase CreatePhase<TPhase>() where TPhase : IPhase;
}

public class PhaseFactory(IServiceProvider serviceProvider) : IPhaseFactory
{
    public TPhase CreatePhase<TPhase>() where TPhase : IPhase
    {
        return serviceProvider.GetRequiredService<TPhase>();
    }
}