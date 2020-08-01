namespace Archetype
{
    public abstract class ActionModifier<THost> : Attachment<THost, ActionInfo>
        where THost : class, IModifierAttachee<THost>
    {
        public int Modifier { get; protected set; }
        public float Multiplier { get; protected set; }

        protected ActionModifier(int modifier, float multiplier)
        {
            Modifier = modifier;
            Multiplier = multiplier;
        }
    }

    public abstract class ActionModifier<THost, TAct> : ActionModifier<THost>
        where THost: class, IModifierAttachee<THost>
        where TAct : ModifiableActionInfo
    {
        protected ActionModifier(int modifier, float multiplier) : base(modifier, multiplier)
        {
        }

        protected override void Handler(object sender, ActionInfo actionInfo)
        {
            if (!(actionInfo is TAct)) return;

            Modify(actionInfo as TAct);
        }
        private void Modify(TAct actionInfo)
        {
            actionInfo.Multiplier += (Multiplier - 1f);
            actionInfo.Modifier += Modifier;
        }
    }
}
