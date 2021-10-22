using System;
using System.Threading.Tasks;
using Archetype.Core;

namespace Archetype.Game
{
    public interface IArchetypeGame
    {
        IObservable<IGameState> GameState { get; }

        Task PlayCardAsync(IPlayCardArgs playCardArgs);
        Task EndTurnAsync();
    }
}