using System.Collections.Generic;
using Archetype.Game.Payloads.Metadata;

namespace Archetype.Core.Data.Instance
{
    public class CardInstance : GamePieceInstance
    {
        public int Cost { get; set; }
        public CardMetaData MetaData { get; set; }
        public string RulesText { get; set; }
        public List<TargetData> Targets { get; set; }
    }
}