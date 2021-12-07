using System;
using System.Linq;

namespace Archetype.Game.Attributes
{
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

    internal class GroupAttribute : Attribute
    {
        public string Description { get; }
        public GroupAttribute(string description)
        {
            Description = description;
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

    internal class TemplateAttribute : Attribute
    {
        public string Template {get;}
        public int NParameters { get; }

        public TemplateAttribute(string template)
        {
            Template = template;

            NParameters = Template.Count(c => c == '{');
        }
    }
}