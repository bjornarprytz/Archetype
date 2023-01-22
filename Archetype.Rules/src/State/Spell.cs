using Archetype.Core.Atoms.Cards;
using Archetype.Core.Proto;

namespace Archetype.Rules.State;

public class Spell : Card, ISpell
{
    // - Can target any atom.
    // - Can essentially do anything.
    // - Go to the graveyard when played
    
    public Spell(IProtoSpell protoSpell) : base(protoSpell)
    {
        
    }
    
    
}