using System.Collections.Generic;
using Archetype.Dto.Composite;

namespace Archetype.Dto.Instance
{
    public class UnitInstance : GamePieceInstance
    {
        public UnitMetaData MetaData { get; set; }
        
        public List<CardInstance> Cards { get; set; }
    }
}