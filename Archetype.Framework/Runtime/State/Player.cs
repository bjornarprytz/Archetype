using Archetype.Framework.Proto;

namespace Archetype.Framework.Runtime.State;

public interface IPlayer : IAtom
{
    
}

public class Player : Atom, IPlayer
{
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "player")
        );
}