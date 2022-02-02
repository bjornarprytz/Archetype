using Archetype.Core.Atoms.Base;
using Archetype.Core.Infrastructure;
using Archetype.Core.Play;
using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;
using Archetype.View.Infrastructure;

namespace Archetype.Core.Atoms;

public interface IPlayer : IGameAtom, IPlayerFront
{
    new IStructure HeadQuarters { get; }
        
    new IDeck Deck { get; }
    new IHand Hand { get; }
        
    IEffectResult<IPlayer, int> Draw(int strength);

    IEffectResult<IPlayer, IStructure> SetHeadQuarters(IStructure structure);
}
    
internal class Player : Atom, IPlayer
{
    private readonly IPlayerData _protoData;

    public Player(IPlayerData protoData)
    {
        _protoData = protoData;

        Resources = _protoData.StartingResources;
            
        Deck = new Deck(this);
        Hand = new Hand(this);
    }
    public int MaxHandSize => _protoData.MaxHandSize;
    public int Resources { get; set; }
    public IStructure HeadQuarters { get; private set; }
    IDeckFront IPlayerFront.Deck => Deck;
    IHandFront IPlayerFront.Hand => Hand;
    IStructureFront IPlayerFront.HeadQuarters => HeadQuarters;

    public IDeck Deck { get; }
    public IHand Hand { get; }

    public IEffectResult<IPlayer, int> Draw(int strength)
    {
        var actualStrength = Math.Clamp(strength, 0, Deck.NumberOfCards);

        var sideEffects = new List<IEffectResult>();

        for (var i=0; i < actualStrength; i++)
        {
            var card = Deck.PopCard();
            sideEffects.Add(card.MoveTo(Hand));
        }

        return ResultFactory.Create(this, actualStrength, sideEffects);
    }

    public IEffectResult<IPlayer, IStructure> SetHeadQuarters(IStructure structure)
    {
        HeadQuarters = structure;
            
        return ResultFactory.Create(this, structure);
    }
}