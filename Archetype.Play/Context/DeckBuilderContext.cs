using System.Reflection.Metadata;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Play.Context;

public interface IDeckBuilderContext
{
    int MinDeckSize { get; }
    
    IEnumerable<IStructureProtoDataFront> AvailableStructures { get; }

    IEnumerable<ICardProtoDataFront> AvailableCards { get; }

    void PickHeadquarter(IStructureProtoDataFront newHq);
    
    void Commit(IEnumerable<ICardProtoDataFront> choices);
}

internal class DeckBuilderContext : IDeckBuilderContext
{
    private readonly IPlayerData _playerData;

    public DeckBuilderContext(IPlayerData playerData)
    {
        _playerData = playerData;
    }

    public int MinDeckSize => _playerData.MinDeckSize;
    public IEnumerable<IStructureProtoData> AvailableStructures => _playerData.StructurePool;
    public IEnumerable<ICardProtoData> AvailableCards => _playerData.CardPool;

    public void PickHeadquarter(IStructureProtoData newHq)
    {
        _playerData.SetHeadQuarters(newHq);
    }

    public void Commit(IEnumerable<ICardProtoData> choices)
    {
        var deck = choices.ToList();

        if (deck.Count < _playerData.MinDeckSize)
            throw new ArgumentException($"Deck list too small ({deck.Count} < {_playerData.MinDeckSize})");

        _playerData.DeckList = choices.ToList();
    }
}