using System.Collections.Generic;
using Archetype.Core.Data.Instance;

namespace Archetype.Core
{
    public class SetData
    {
        public string Name { get; set; }
        public List<CardInstance> Cards { get; set; } = new();
    }

}
