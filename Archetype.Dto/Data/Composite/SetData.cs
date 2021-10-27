using System.Collections.Generic;

namespace Archetype.Core
{
    public class SetData
    {
        public string Name { get; set; }
        public List<CardData> Cards { get; set; } = new();
    }

}
