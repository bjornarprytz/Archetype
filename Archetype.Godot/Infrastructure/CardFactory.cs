
using Archetype.Game.Payloads.Pieces;
using Archetype.Godot.Card;
using Godot;

namespace Archetype.Godot.Infrastructure
{
    public interface ICardFactory
    {
        CardNode CreateCard(ICard card);
    }
    
    public class CardFactory : ICardFactory
    {
        private readonly PackedScene _cardScene;

        public CardFactory()
        {
            _cardScene = ResourceLoader.Load<PackedScene>("res://card.tscn") 
                         ?? throw new MissingPackedSceneException("res://card.tscn");
        }
        
        public CardNode CreateCard(ICard card)
        {
            var cardNode = _cardScene.Instance() as CardNode;
            
            cardNode!.Construct(card);

            return cardNode;
        }
    }
}