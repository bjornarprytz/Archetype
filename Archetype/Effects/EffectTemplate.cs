using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public abstract class EffectTemplate
    {
        public virtual ChooseTargets Requirements { get; protected set; }

        protected EffectTemplate(ChooseTargets requirements)
        {
            Requirements = requirements;
        }

        public abstract Effect CreateEffect(Unit source, Decision userInput);

        protected List<Unit> HandleUserInput(Decision userInput)
        {
            if (!userInput.Meets(Requirements)) throw new Exception("User input insufficient to create effect");

            return new List<Unit>(userInput.ChosenPieces.Select(piece => (Unit)piece));
        }
    }
}
