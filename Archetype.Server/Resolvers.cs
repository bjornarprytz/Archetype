using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Game.Payloads.PlayContext;
using Archetype.Game.Payloads.Proto;
using HotChocolate.Types;

namespace Archetype.Server
{
    public class GameStateType : ObjectType<GameState>
    {
        protected override void Configure(IObjectTypeDescriptor<GameState> descriptor)
        {
            descriptor.Implements<InterfaceType<IGameState>>();
        }
    }
    
    public class PlayerType : ObjectType<Player>
    {
        protected override void Configure(IObjectTypeDescriptor<Player> descriptor)
        {
            descriptor.Implements<InterfaceType<IPlayer>>();
        }
    }
    public class MapType : ObjectType<Map>
    {
        protected override void Configure(IObjectTypeDescriptor<Map> descriptor)
        {
            descriptor.Implements<InterfaceType<IMap>>();
        }
    }
    
    public class MapNodeType : ObjectType<MapNode>
    {
        protected override void Configure(IObjectTypeDescriptor<MapNode> descriptor)
        {
            descriptor.Implements<InterfaceType<IMapNode>>();
        }
    }

    public class CardPoolType : ObjectType<CardPool>
    {
        protected override void Configure(IObjectTypeDescriptor<CardPool> descriptor)
        {
            descriptor.Implements<InterfaceType<ICardPool>>();
        }
    }

    public class CardSetType : ObjectType<CardSet>
    {
        protected override void Configure(IObjectTypeDescriptor<CardSet> descriptor)
        {
            descriptor.Implements<InterfaceType<ICardSet>>();
        }
    }

    public class DeckType : ObjectType<Deck>
    {
        protected override void Configure(IObjectTypeDescriptor<Deck> descriptor)
        {
            descriptor.Implements<InterfaceType<IDeck>>();
        }
    }
    
    public class HandType : ObjectType<Hand>
    {
        protected override void Configure(IObjectTypeDescriptor<Hand> descriptor)
        {
            descriptor.Implements<InterfaceType<IHand>>();
        }
    }
    
    public class DiscardPileType : ObjectType<DiscardPile>
    {
        protected override void Configure(IObjectTypeDescriptor<DiscardPile> descriptor)
        {
            descriptor.Implements<InterfaceType<IDiscardPile>>();
        }
    }
    
    public class CardProtoDataType : ObjectType<CardProtoData>
    {
        protected override void Configure(IObjectTypeDescriptor<CardProtoData> descriptor)
        {
            descriptor.Implements<InterfaceType<ICardProtoData>>();
        }
    }
    
    public class UnitType : ObjectType<Unit>
    {
        protected override void Configure(IObjectTypeDescriptor<Unit> descriptor)
        {
            descriptor.Description("A unit in the game");
            descriptor.Implements<InterfaceType<IUnit>>();
        }
    }
    
    public class CardType : ObjectType<Card>
    {
        protected override void Configure(IObjectTypeDescriptor<Card> descriptor)
        {
            descriptor.Description("A card in the game");

            descriptor.Implements<InterfaceType<ICard>>();
        }
        
        private class Resolvers
        {
            public IEnumerable<string> GetTargets(IEnumerable<ITarget> targets) => targets.Select(t => t.TargetType.Name);
        }
    }
    
    public class TargetType : ObjectType<ITarget>
    {
        protected override void Configure(IObjectTypeDescriptor<ITarget> descriptor)
        {
            descriptor.Description("A target of some type");
            
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
            descriptor.Description("An effect of a card");

            descriptor
                .Field(effect => effect.ResolveContext(default!))
                .Ignore();

            descriptor
                .Field(effect => effect.CallTextMethod(default!))
                .Ignore();
        }
    }
    
    

}