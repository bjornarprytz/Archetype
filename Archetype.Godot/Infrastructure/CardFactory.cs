using Archetype.Client;
using Archetype.Godot.Card;
using Godot;

namespace Archetype.Godot.Infrastructure
{
    public interface ICardFactory
    {
        CardNode CreateCard(ICardProtoData card);
    }
    
    public class CardFactory : ICardFactory
    {
        private readonly PackedScene _cardScene;
        private readonly IArchetypeGraphQLClient _client;

        public CardFactory(IArchetypeGraphQLClient client)
        {
            _cardScene = ResourceLoader.Load<PackedScene>("res://card.tscn") 
                         ?? throw new MissingPackedSceneException("res://card.tscn");
            _client = client;
        }
        
        public CardNode CreateCard(ICardProtoData card)
        {
            var cardNode = _cardScene.Instance() as CardNode;
            
            cardNode!.Construct(card, _client);

            return cardNode;
        }
    }
}