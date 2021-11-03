using System.Collections.Generic;
using Archetype.Core.Data.Instance;

namespace Archetype.Core.Data.Composite
{
    public class MapNodeData : GamePieceInstance
    {
        public List<CardInstance> Cards { get; set; } = new();
        public List<UnitInstance> Enemies { get; set; } = new();
        
        public List<MapNodeData> Neighbours { get; set; } = new();
    }
}