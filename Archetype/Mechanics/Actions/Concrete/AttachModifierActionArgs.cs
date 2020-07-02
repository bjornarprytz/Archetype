using System;

namespace Archetype
{
    public class AttachModifierActionArgs : ActionInfo
    {
        public ActionModifier<Unit> Modifier { get; set; }

        public AttachModifierActionArgs(ISource source, Unit target, ActionModifier<Unit> modifier) : base(source, target)
        {
            Modifier = modifier;
        }

        protected override void Resolve()
        {
            (Target as Unit).AttachModifier(Modifier);
        }
    }
}
