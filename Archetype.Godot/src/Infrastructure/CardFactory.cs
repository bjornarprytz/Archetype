using Archetype.Godot.Card;
using Archetype.Prototype1Data;
using Godot;

namespace Archetype.Godot.Infrastructure
{
    public interface ICardFactory
    {
        CardNode CreateCard(ICard card);
    }
    
    public class CardFactory : ICardFactory
    {
        private readonly ISceneFactory _sceneFactory;

        public CardFactory(ISceneFactory sceneFactory)
        {
            _sceneFactory = sceneFactory; 
        }
        
        public CardNode CreateCard(ICard card)
        {
            var cardNode = _sceneFactory.CreateNode<CardNode>();
            
            cardNode!.Load(card);

            return cardNode;
        }
    }
}