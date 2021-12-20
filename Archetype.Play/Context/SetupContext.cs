using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Play.Context;

public interface ISetupContext
{
    IProtoPool ProtoPool { get; }
    IMap Map { get; }
    ITurnContext Start(IEnumerable<string> cards, string hqName, IMapNode hqPlacement);
}

public class SetupContext : ISetupContext
{
    private readonly ITurnContext _turnContext;
    private readonly IPlayer _player;
    private readonly IInstanceFactory _instanceFactory;
    private readonly IInstanceFinder _instanceFinder;

    public SetupContext(
        ITurnContext turnContext, 
        IProtoPool protoPool, 
        IPlayer player, 
        IMap map, 
        IInstanceFactory instanceFactory, 
        IInstanceFinder instanceFinder
        )
    {
        ProtoPool = protoPool;
        Map = map;
        _turnContext = turnContext;
        _player = player;
        _instanceFactory = instanceFactory;
        _instanceFinder = instanceFinder;
    }
    
    public IProtoPool ProtoPool { get; }
    public IMap Map { get; }
    
    public ITurnContext Start(IEnumerable<string> cards, string hqName, IMapNode hqPlacement)
    {
        var result = hqPlacement.CreateStructure(hqName, _player);

        _player.SetHeadquarters( result.Result);
            
        foreach (var name in cards)
        {
            var card = _instanceFactory.CreateCard(name, _player);
                
            _player.Deck.PutCardOnTop(card);
        }
            
        _player.Deck.Shuffle();

        if (_player.Deck.Contents.Count() < _player.MinDeckSize)
            throw new InvalidOperationException("Deck is too small");
            
        _player.Draw(_player.MaxHandSize);

        return _turnContext;
    }
}