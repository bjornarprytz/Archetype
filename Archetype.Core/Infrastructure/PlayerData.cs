using Archetype.Core.Play;
using Archetype.Core.Proto;
using Archetype.View.Proto;

namespace Archetype.Core.Infrastructure;

public interface IPlayerData : IPlayerDataFront // TODO: Revisit this whole design
{
    new IStructureProtoData Headquarters { get; }
    new IEnumerable<IStructureProtoData> StructurePool { get; }
    new IEnumerable<ICardProtoData> CardPool { get; }
    new IEnumerable<ICardProtoData> DeckList { get; }

    IEffectResult<IStructureProtoData> SetHeadQuarters(IStructureProtoData newHq);
    IEffectResult<ICardProtoData> AddToCardPool(ICardProtoData card);
    IEffectResult<IStructureProtoData> AddToStructurePool(IStructureProtoData structure);

    void SubmitDeck(IEnumerable<ICardProtoData> deckList);
}
    
internal class PlayerData : IPlayerData
{
    private readonly List<ICardProtoData> _cardPool = new();
    private readonly List<IStructureProtoData> _structurePool = new();
    private readonly List<ICardProtoData> _deckList = new();

    public int StartingResources { get; } = 100;
    public int MaxHandSize { get; } = 2;
    public int MinDeckSize { get; } = 4;

    IStructureProtoDataFront IPlayerDataFront.Headquarters => Headquarters;
    IEnumerable<IStructureProtoDataFront> IPlayerDataFront.StructurePool => StructurePool;

    IEnumerable<ICardProtoDataFront> IPlayerDataFront.CardPool => CardPool;

    IEnumerable<ICardProtoDataFront> IPlayerDataFront.DeckList => DeckList;

    public IStructureProtoData Headquarters { get; private set; }
    public IEnumerable<IStructureProtoData> StructurePool => _structurePool;
    public IEnumerable<ICardProtoData> CardPool => _cardPool;
    public IEnumerable<ICardProtoData> DeckList => _deckList;
        
    public IEffectResult<IStructureProtoData> SetHeadQuarters(IStructureProtoData newHq)
    {
        Headquarters = newHq;
            
        return ResultFactory.Create(newHq);
    }
        
    public IEffectResult<ICardProtoData> AddToCardPool(ICardProtoData card)
    {
        _cardPool.Add(card);
            
        return ResultFactory.Create(card);
    }
        
    public IEffectResult<IStructureProtoData> AddToStructurePool(IStructureProtoData structure)
    {
        _structurePool.Add(structure);
            
        return ResultFactory.Create(structure);
    }

    public void SubmitDeck(IEnumerable<ICardProtoData> deckList)
    {
        _deckList.Clear();
            
        _deckList.AddRange(deckList);
    }
}