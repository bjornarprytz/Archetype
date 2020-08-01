namespace Archetype
{
    public class ModifierData : AttachmentData
    {
        public ModifiableActionType ActionType { get; set; }

        public override ActionInfo GetAttachmentActionInfo(ISource source, ITarget target, GameState gameState)
        {
            throw new System.NotImplementedException();
        }
    }
}
