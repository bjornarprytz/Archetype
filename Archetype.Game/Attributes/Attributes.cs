using System;

namespace Archetype.Game.Attributes
{
    internal class RulesDescriptionAttribute : Attribute
    {
        public string Word { get; }
        
        public RulesDescriptionAttribute(string word)
        {
            Word = word;
        }
    }
    
    internal class TargetAttribute : Attribute
    {
        public string Singular { get; }
        public string Plural { get; }

        public TargetAttribute(string singular, string plural=null)
        {
            Singular = singular;
            Plural = plural ?? $"{singular}s";
        }
    }

    internal class VerbAttribute : Attribute{

        public string Name {get;}
        public string Preposition {get;}

        public VerbAttribute(string name, string preposition=null)
        {
            Name = name;
            Preposition = preposition ?? "by";
        }
    }
}