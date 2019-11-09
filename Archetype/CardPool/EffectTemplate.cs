using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public abstract class EffectTemplate
    {
        public abstract string RulesText { get; }
        public abstract PromptRequirements Requirements { get; protected set; }
        public abstract Effect CreateEffect(Unit source, PromptResult userInput);
    }
}
