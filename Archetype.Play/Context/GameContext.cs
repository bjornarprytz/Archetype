using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Proto;
using Archetype.Play.Factory;

namespace Archetype.Play.Context;


public interface IGameContext
{
    // State
    
    
    // Actions
    ISetupContext Setup();
    IDeckBuilderContext BuildDeck();

    void BootStrap();
}

internal class GameContext : IGameContext
{
    private readonly IFactory<ISetupContext> _setupContextFactory;
    private readonly IFactory<IDeckBuilderContext> _deckBuilderContextFactory;
    private readonly IPlayerData _playerData;
    private readonly IProtoPool _protoPool;


    public GameContext(
        IFactory<ISetupContext> setupContextFactory,
        IFactory<IDeckBuilderContext> deckBuilderContextFactory,
        IPlayerData playerData,
        IProtoPool protoPool)
    {
        _setupContextFactory = setupContextFactory;
        _deckBuilderContextFactory = deckBuilderContextFactory;
        _playerData = playerData;
        _protoPool = protoPool;
    }
    public ISetupContext Setup()
    {
        return _setupContextFactory.Create();
    }

    public IDeckBuilderContext BuildDeck()
    {
        return _deckBuilderContextFactory.Create();
    }

    public void BootStrap()
    {
        var structure = _protoPool.GetStructure("House"); 
        
        _playerData.AddToStructurePool(structure);

        _playerData.SetHeadQuarters(structure);

        foreach (var card in _protoPool.Cards)
        {
            _playerData.AddToCardPool(card);
        }

        _playerData.DeckList = _playerData.CardPool;
    }
}