using System;
using System.Linq;

namespace Archetype.Game.Attributes
{
    internal class PropertyShortHandAttribute : Attribute
    {
        public string Path { get; }

        public PropertyShortHandAttribute(string path)
        {
            Path = path;
        }
    }
    
    internal class ContextFactAttribute : Attribute
    {
        public string Description { get; }

        public ContextFactAttribute(string description)
        {
            Description = description;
        }
    }

    internal class KeywordAttribute : Attribute
    {
        public string Name {get;}

        public KeywordAttribute(string name)
        {
            Name = name;
        }
    }
}