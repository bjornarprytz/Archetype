using System;

namespace Archetype
{
    public interface ITriggerHandlerFactory
    {
        EventHandler<TriggerArgs> CreateTriggerHandler(ISource source, ITarget target, GameState gameState);
    }
}
