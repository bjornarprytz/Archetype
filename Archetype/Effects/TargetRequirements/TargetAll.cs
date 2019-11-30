using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    class TargetAll<T> : TargetParams<T> where T : Unit
    {   
        public TargetAll(TargetPredicate<T> predicate) : base (predicate) { }

        internal override PromptResponse GetTargets(Unit owner, IEnumerable<T> options, RequiredAction actionPrompt)
        {
            return PromptResponse.Choose(options.Where(o => _predicate(owner, o)).ToList());
        }
    }
}
