using System.Collections.Generic;
using Archetype.View.Atoms;
using Archetype.View.Infrastructure;

namespace Archetype.Game;

public interface IArchetypeGame
{
    IGameStateFront GameState { get; }
    IEnumerable<ICardFront> PlayableCards { get; }
}