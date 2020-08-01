namespace Archetype
{
    public abstract class AttachmentData
    {
        public abstract ActionInfo GetAttachmentActionInfo(ISource source, ITarget target, GameState gameState);
    }
}
