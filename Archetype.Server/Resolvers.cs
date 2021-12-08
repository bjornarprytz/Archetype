using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Context.Effect;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Game.Payloads.Proto;
using Archetype.Server.Extensions;
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
    
    public class UnitType : ObjectType<Unit>
    {
        protected override void Configure(IObjectTypeDescriptor<Unit> descriptor)
        {
            descriptor.Description("A unit instance");
            descriptor.Implements<InterfaceType<IUnit>>();
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
            public string RulesText(Card card, [Service] IProtoPool protoPool) => protoPool.GetCard(card.ProtoGuid).RulesText;
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
    
    public class EffectType : ObjectType<IEffect>
    {
        protected override void Configure(IObjectTypeDescriptor<IEffect> descriptor)
        {
            descriptor.Description("The core payload of a card, where mutation of game atoms happen");

            descriptor
                .Field(nameof(IEffect.ResolveContext))
                .Ignore();
            
            descriptor.Ignore(e => e.PrintedRulesText());

            descriptor.Ignore(e => e.ContextSensitiveRulesText(default!));

        }
    }
    
    

}