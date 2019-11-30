using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public abstract class TargetParams<T> where T : GamePiece
    {
        protected TargetPredicate<T> _predicate;

        protected TargetParams(TargetPredicate<T> predicate)
        {
            _predicate = predicate;
        }

        internal abstract PromptResponse GetTargets(Unit owner, IEnumerable<T> options, RequiredAction actionPrompt);

        protected bool HasValidTargets(Unit owner, IEnumerable<T> options) => options.Any(p => _predicate(owner, p));

        protected List<T> GetValidTargets(Unit owner, IEnumerable<T> options) => options.Where(p => _predicate(owner, p)).ToList();
        
    }
}
