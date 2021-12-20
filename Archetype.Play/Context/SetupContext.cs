using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Proto;
using Archetype.Play.Factory;

namespace Archetype.Play.Context;

public interface ISetupContext
{
    IMap Map { get; }
    ITurnContext Start(IMapNode hqPlacement);
}

public class SetupContext : ISetupContext
{
    private readonly IFactory<ITurnContext> _turnContextFactory;
    private readonly IPlayerData _playerData;
    private readonly IPlayer _player;
    private readonly IInstanceFactory _instanceFactory;

    public SetupContext(
        IPlayerData playerData,
        IPlayer player,
        IMap map,
        IFactory<ITurnContext> turnContextFactory, 
        IInstanceFactory instanceFactory
        )
    {
        Map = map;
        _turnContextFactory = turnContextFactory;
        _playerData = playerData;
        _player = player;
        _instanceFactory = instanceFactory;
    }
    public IMap Map { get; }
    
    public ITurnContext Start(IMapNode hqPlacement)
    {
        _player.HeadQuarters.MoveTo(hqPlacement);

        foreach (var cardProtoData in _playerData.DeckList)
        {
            var card = _instanceFactory.CreateCard(cardProtoData, _player);
            
            _player.Deck.PutCardOnTop(card);
        }
        
        _player.Deck.Shuffle();

        _player.Draw(_player.MaxHandSize);

        return _turnContextFactory.Create();
    }
}