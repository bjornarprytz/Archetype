using System.Collections.Generic;
using Archetype.Dto.MetaData;

namespace Archetype.Dto.Instance
{
    public class UnitInstance : GamePieceInstance
    {
        public UnitMetaData MetaData { get; set; }
        
        public List<CardInstance> Cards { get; set; }
    }
}