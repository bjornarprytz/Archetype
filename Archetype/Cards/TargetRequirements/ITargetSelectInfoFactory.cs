namespace Archetype
{
    public interface ITargetSelectInfoFactory
    {
        ITargetSelectInfo GetTargetInfo(Unit source, GameState gameState);
    }
}
