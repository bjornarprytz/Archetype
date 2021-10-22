using Archetype.Core;

namespace Archetype.Game
{
    public interface IPlayCardArgs
    {
        ICard Card { get; }
        ICardArgs Args { get; }
    }
}