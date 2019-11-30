
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    class TargetSelf : TargetParams<Unit>
    {
        public TargetSelf() : base((s, t) => t.Id == s.Id) { }

        internal override PromptResponse GetTargets(Unit owner, IEnumerable<Unit> options, RequiredAction actionPrompt)
        {
            return PromptResponse.Choose(options.Where(o => _predicate(owner, o)).ToList());
        }
    }
}
