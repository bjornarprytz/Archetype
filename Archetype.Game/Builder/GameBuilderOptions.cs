using System;
using Archetype.Core;

namespace Archetype.Game
{
    public class GameBuilderOptions
    {
        public GameBuilderOptions(Action<ICardPool> cardPoolProvider)
        {
            CardPoolProvider = cardPoolProvider;
        }

        public Action<ICardPool> CardPoolProvider { get; }
    }
}