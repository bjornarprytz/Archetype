namespace Archetype
{
    public interface ITargetSelectionInfoFactory
    {
        ISelectionInfo<ITarget> GetTargetInfo(Unit source, GameState gameState);
    }
}
