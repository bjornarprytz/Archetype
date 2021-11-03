using System.Linq;
using Archetype.Core;
using Archetype.Core.Data.Instance;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Server.Extensions
{
    public static class MappingExtensions
    {
        public static CardInstance CreateDto(this ICard card)
        {
            return new CardInstance
            {
                Cost = card.Cost,
                MetaData = card.ProtoData.MetaData,
                RulesText = card.GenerateRulesText(),
                Targets = card.Targets.Select(t => t.CreateDto()).ToList()
            };
        }
        
        public static TargetData CreateDto(this ITarget target)
        {
            return new TargetData
            {
                TargetType = target.TargetType
            };
        }

        public static string GenerateRulesText(this ICard card)
        {
            // TODO: take context into consideration

            return card.Effects.Select(e => e.CallTextMethod(null))
                .Aggregate(((s, s1) => s + s1));
        }
    }
}