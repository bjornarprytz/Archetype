namespace Archetype
{
    public interface ITargetSelectionInfoFactory
    {
        ISelectionInfo<ITarget> GetSelectionInfo(Unit source, GameState gameState);
    }
}
