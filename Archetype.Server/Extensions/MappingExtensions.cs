using System.Linq;
using Archetype.Dto.Instance;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.PlayContext;

namespace Archetype.Server.Extensions
{
    public static class MappingExtensions
    {

        public static string GenerateRulesText(this ICard card, IGameState gameState)
        {
            return card.Effects.Select(e => e.CreateRulesText(gameState))
                .Aggregate(((s, s1) => s + s1));
        }
        
        public static string GenerateRulesText(this ICard card)
        {
            // TODO: take context into consideration

            return card.Effects.Select(e => e.CreateRulesText())
                .Aggregate(((s, s1) => s + s1));
        }
    }
}