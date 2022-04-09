namespace Archetype.Prototype2Data.Base;

internal abstract class GameAtom : IGameAtom
{
    protected GameAtom()
    {
        Id = Guid.NewGuid();
    }
    
    public Guid Id { get; }
}
