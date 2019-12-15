using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public abstract class ChoiceArgs
    {
        public abstract bool Valid { get; }
    }

    public abstract class MultipleChoicesArgs<C> : ChoiceArgs where C : ChoiceArgs
    {
        protected List<C> SubChoices { get; set; }
        public override bool Valid => SubChoices.All(arg => arg.Valid);
    }
}
