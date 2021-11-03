using System.Collections.Generic;
using Archetype.Game.Payloads.Metadata;

namespace Archetype.Core.Data.Instance
{
    public class UnitInstance : GamePieceInstance
    {
        public UnitMetaData MetaData { get; set; }
        
        public List<CardInstance> Cards { get; set; }
    }
}