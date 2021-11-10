using System.Collections.Generic;
using Archetype.Dto.MetaData;

namespace Archetype.Dto.Instance
{
    public class CardInstance : GamePieceInstance
    {
        public int Cost { get; set; }
        public CardMetaData MetaData { get; set; }
        public string RulesText { get; set; }
        public List<TargetData> Targets { get; set; }
    }
}