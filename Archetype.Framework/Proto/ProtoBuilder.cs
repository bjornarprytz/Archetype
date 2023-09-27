using Archetype.Framework.Definitions;

namespace Archetype.Framework.Proto;


public class ProtoBuilder
{
    private ProtoCard _card;

    
    
    public ProtoCard Build()
    {
        // TODO: Validate card
        // TODO: Add type specific effects and conditions
        // e.g. units always target nodes, and put themselves into play
        // spells go to the discard pile
        
        // NOTE: This could be a pluggable behaviour, because it's encroaching on the game design domain
        
        return _card;
    }
    
    public void AddStaticKeyword(KeywordInstance keywordInstance)
    {
        // TODO: Ensure it's a static keyword
    }
    
    public void AddEffects(IReadOnlyList<TargetDescription> targets, IReadOnlyList<ComputedValueInstance> computedValues, IReadOnlyList<EffectInstance> effects)
    {
        
    }
    
}