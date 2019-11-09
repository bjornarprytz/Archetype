using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public abstract class EffectTemplate
    {
        public abstract string RulesText { get; }
        public virtual PromptRequirements Requirements { get; protected set; }

        protected EffectTemplate(PromptRequirements requirements)
        {
            Requirements = requirements;
        }

        public abstract Effect CreateEffect(Unit source, PromptResult userInput);

        protected List<Unit> HandleUserInput(PromptResult userInput)
        {
            if (!userInput.Meets(Requirements)) throw new Exception("User input insufficient to create effect");

            return new List<Unit>(userInput.ChosenPieces.Select(piece => (Unit)piece));
        }
    }
}
