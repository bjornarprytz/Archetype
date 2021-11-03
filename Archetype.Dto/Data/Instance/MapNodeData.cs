using System.Collections.Generic;

namespace Archetype.Core.Data.Instance
{
    public class MapNodeData : GamePieceInstance
    {
        public List<CardInstance> Cards { get; set; } = new();
        public List<UnitInstance> Enemies { get; set; } = new();
        
        public List<MapNodeData> Neighbours { get; set; } = new();
    }
}