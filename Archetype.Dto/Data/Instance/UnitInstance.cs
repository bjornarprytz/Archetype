using System.Collections.Generic;
using Archetype.Core.Data.Composite;

namespace Archetype.Core.Data.Instance
{
    public class UnitInstance : GamePieceInstance
    {
        public UnitMetaData MetaData { get; set; }
        
        public List<CardInstance> Cards { get; set; }
    }
}