namespace Archetype
{
    public class AttachAttachmentParameterData : ActionParameterData
    {
        public AttachmentData Attachment { get; set; }

        protected override ActionInfo GetActionInfo(ISource source, ITarget target, GameState gameState)
        {
            return Attachment.GetAttachmentActionInfo(source, target, gameState);
        }
    }
}
