using System.Collections.Generic;

namespace Archetype.Dto.Instance
{
    public class SetData
    {
        public string Name { get; set; }
        public List<CardInstance> Cards { get; set; } = new();
    }

}
