using System;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Context.Effect;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Game.Payloads.Proto;
using HotChocolate;
using HotChocolate.Types;

namespace Archetype.Server
{
    public class GameStateType : ObjectType<GameState>
    {
        protected override void Configure(IObjectTypeDescriptor<GameState> descriptor)
        {
            descriptor.Description("The root object of actionable game state");
            descriptor.Implements<InterfaceType<IGameState>>();
        }
    }
    
    public class PlayerType : ObjectType<Player>
    {
        protected override void Configure(IObjectTypeDescriptor<Player> descriptor)
        {
            descriptor.Description("The player of the game");
            descriptor.Implements<InterfaceType<IPlayer>>();
        }
    }
    public class MapType : ObjectType<Map>
    {
        protected override void Configure(IObjectTypeDescriptor<Map> descriptor)
        {
            descriptor.Description("A graph of map nodes");
            descriptor.Implements<InterfaceType<IMap>>();
        }
    }
    
    public class MapNodeType : ObjectType<MapNode>
    {
        protected override void Configure(IObjectTypeDescriptor<MapNode> descriptor)
        {
            descriptor.Description("A node on the map");
            descriptor.Implements<InterfaceType<IMapNode>>();

            descriptor
                .Field(node => node.CreateCreature(default!, default!))
                .Ignore();
            
            descriptor
                .Field(node => node.CreateStructure(default!, default!))
                .Ignore();
        }
    }

    public class CardPoolType : ObjectType<ProtoPool>
    {
        protected override void Configure(IObjectTypeDescriptor<ProtoPool> descriptor)
        {
            descriptor.Description("Collection of all available card sets");
            descriptor.Implements<InterfaceType<IProtoPool>>();
        }
    }

    public class CardSetType : ObjectType<Set>
    {
        protected override void Configure(IObjectTypeDescriptor<Set> descriptor)
        {
            descriptor.Description("A set of card proto data which share some themes");
            descriptor.Implements<InterfaceType<ISet>>();
        }
    }

    public class DeckType : ObjectType<Deck>
    {
        protected override void Configure(IObjectTypeDescriptor<Deck> descriptor)
        {
            descriptor.Description("A stack of cards which replenishes the player's hand");
            descriptor.Implements<InterfaceType<IDeck>>();
        }
    }
    
    public class HandType : ObjectType<Hand>
    {
        protected override void Configure(IObjectTypeDescriptor<Hand> descriptor)
        {
            descriptor.Description("A hand of cards, only visible to the owner");
            descriptor.Implements<InterfaceType<IHand>>();
        }
    }
    
    public class DiscardPileType : ObjectType<DiscardPile>
    {
        protected override void Configure(IObjectTypeDescriptor<DiscardPile> descriptor)
        {
            descriptor.Description("Discard pile, where spent cards go");
            descriptor.Implements<InterfaceType<IDiscardPile>>();
        }
    }
    
    public class CardProtoDataType : ObjectType<CardProtoData>
    {
        protected override void Configure(IObjectTypeDescriptor<CardProtoData> descriptor)
        {
            descriptor.Description("Blueprint for creating card instances");
            descriptor.Implements<InterfaceType<ICardProtoData>>();
        }
    }
    public class StructureProtoDataType : ObjectType<StructureProtoData>
    {
        protected override void Configure(IObjectTypeDescriptor<StructureProtoData> descriptor)
        {
            descriptor.Description("Blueprint for creating a Structure instance");
            descriptor.Implements<InterfaceType<IStructureProtoData>>();
        }
    }
    
    public class CreatureProtoDataType : ObjectType<CreatureProtoData>
    {
        protected override void Configure(IObjectTypeDescriptor<CreatureProtoData> descriptor)
        {
            descriptor.Description("Blueprint for creating a Creature instance");
            descriptor.Implements<InterfaceType<ICreatureProtoData>>();
        }
    }
    
    

    public class StructureType : ObjectType<Structure>
    {
        protected override void Configure(IObjectTypeDescriptor<Structure> descriptor)
        {
            descriptor.Description("An instance of a Structure");
            descriptor.Implements<InterfaceType<IStructure>>();
        }
    }
    
    public class CreatureType : ObjectType<Creature>
    {
        protected override void Configure(IObjectTypeDescriptor<Creature> descriptor)
        {
            descriptor.Description("An instance of a Creature");
            descriptor.Implements<InterfaceType<ICreature>>();
        }
    }
    
    public class CardType : ObjectType<Card>
    {
        protected override void Configure(IObjectTypeDescriptor<Card> descriptor)
        {
            descriptor.Description("A card instance");
            descriptor.Implements<InterfaceType<ICard>>();

            descriptor.Field("contextRulesText")
                .ResolveWith<Resolvers>(resolvers => resolvers.RulesTextWithContext(default!, default!));
            
            descriptor.Field("rulesText")
                .ResolveWith<Resolvers>(resolvers => resolvers.RulesText(default!, default!));

            descriptor.Field(card => card.GenerateRulesText(default!))
                .Ignore();
        }
        
        private class Resolvers
        {
            public string RulesTextWithContext(Card card, [Service] IGameState gameState) => card.GenerateRulesText(gameState);
            public string RulesText(Card card, [Service] IProtoPool protoPool) => protoPool.GetCard(card.Name).RulesText;
        }
    }
    
    public class TargetType : ObjectType<ITarget>
    {
        protected override void Configure(IObjectTypeDescriptor<ITarget> descriptor)
        {
            descriptor.Description("The target of a card");
            
            descriptor
                .Field(target => target.ValidateContext(default!))
                .Ignore();
        }
        
        private class Resolvers
        {
            public string GetTypeName(Type targetType) => targetType.Name;
        }
    }
    
    public class CardEffectType : ObjectType<CardEffect>
    {
        protected override void Configure(IObjectTypeDescriptor<CardEffect> descriptor)
        {
            descriptor.Description("The core payload of a card, where mutation of game atoms happen");

            descriptor
                .Field(nameof(CardEffect.ResolveContext))
                .Ignore();
            
            descriptor.Ignore(e => e.PrintedRulesText());

            descriptor.Ignore(e => e.ContextSensitiveRulesText(default!));

        }
    }

    public class StructureUpkeepEffectType : ObjectType<TriggerEffect<IStructure>>
    {
        protected override void Configure(IObjectTypeDescriptor<TriggerEffect<IStructure>> descriptor)
        {
            descriptor.Description("The core payload of upkeep effects of Structures");
        }
    }
    
    

}