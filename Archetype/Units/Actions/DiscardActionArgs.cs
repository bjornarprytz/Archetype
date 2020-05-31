using System.Security.Cryptography.X509Certificates;

namespace Archetype
{
    public class DiscardActionArgs : ActionInfo
    {
        private IPromptable _prompter;

        public DiscardActionArgs(Unit source, Unit target, int strength, IPromptable prompter) : base(source, target, strength)
        {
            _prompter = prompter;
        }

        protected override void Resolve()
        {
            (Target as Unit).Discard(Strength, _prompter);
        }
    }
}
