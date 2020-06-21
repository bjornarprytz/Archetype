using System;

namespace Archetype
{
    public class DiscardActionArgs : ParameterizedActionInfo<int>
    {
        private IPromptable _prompter;

        public DiscardActionArgs(Unit source, Unit target, Func<int> getter, IPromptable prompter) : base(source, target, getter)
        {
            _prompter = prompter;
        }

        protected override void Resolve()
        {
            (Target as Unit).Discard(Strength, _prompter);
        }
    }
}
