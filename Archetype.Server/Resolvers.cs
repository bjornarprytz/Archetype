using System;
using System.Security.Cryptography.X509Certificates;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Context.Effect.Base;
using Archetype.Game.Payloads.Context.Trigger;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Game.Payloads.Proto;
using HotChocolate.Types;

namespace Archetype.Server
{

    public abstract class Archetype<T> : ObjectType<T>
    {
        protected override void Configure(IObjectTypeDescriptor<T> descriptor)
        {
            base.Configure(descriptor);
        }
    }
    public class GameStateType : Archetype<IGameState>
    {
        protected override void Configure(IObjectTypeDescriptor<IGameState> descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Description("The root object of actionable game state");

            descriptor.BindFieldsExplicitly();

            descriptor.Field("Test").Type<StringType>().Resolve("hey");

            descriptor.Field(state => state.HistoryReader)
                .Ignore(); // TODO: Do not ignore this
        }
    }

    public class AtomType : Archetype<IGameAtom>
    {
        protected override void Configure(IObjectTypeDescriptor<IGameAtom> descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Description("The base class of all game object instances");
        }
    }
    
    public class PlayerType : Archetype<IPlayer>
    {
        protected override void Configure(IObjectTypeDescriptor<IPlayer> descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Description("The player of the game");

            descriptor.Field(player => player.Draw(default!))
                .Ignore();
        }
    }
    
    public class MapType : Archetype<IMap>
    {
        protected override void Configure(IObjectTypeDescriptor<IMap> descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Description("A graph of map nodes");
        }
    }

    public class MapNodeType : Archetype<IMapNode>
    {
        protected override void Configure(IObjectTypeDescriptor<IMapNode> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("A node on the map");
            
            descriptor.Field(node => node.Contents)
                .Type<ListType<UnitUnionType>>();

            descriptor.Field(node => node.Neighbours)
                .Ignore();// TODO: Don't ignore this

            descriptor
                .Field(node => node.CreateCreature(default!, default!))
                .Ignore();
            
            descriptor
                .Field(node => node.CreateStructure(default!, default!))
                .Ignore();

            descriptor.Field(node => node.GetGamePiece(default!))
                .Ignore();
        }
    }

    public class GraveyardType : Archetype<IGraveyard>
    {
        protected override void Configure(IObjectTypeDescriptor<IGraveyard> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("A graveyard that contains dead creatures");
            
            descriptor.Field(graveyard =>  graveyard.Contents)
                .Type<ListType<CreatureType>>();
        }
    }
    
    public class DiscardPileType : Archetype<IDiscardPile>
    {
        protected override void Configure(IObjectTypeDescriptor<IDiscardPile> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("Discard pile, where spent cards go");

            descriptor.Field(pile => pile.Contents)
                .Type<ListType<CardType>>();
        }
    }

    public class PoolType : Archetype<IProtoPool>
    {
        protected override void Configure(IObjectTypeDescriptor<IProtoPool> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("Collection of all available sets");
        }
    }

    public class SetType : Archetype<ISet>
    {
        protected override void Configure(IObjectTypeDescriptor<ISet> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("A set of proto data which share some themes");
        }
    }

    public class DeckType : Archetype<IDeck>
    {
        protected override void Configure(IObjectTypeDescriptor<IDeck> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("A stack of cards which replenishes the player's hand");
        }
    }
    
    public class HandType : Archetype<IHand>
    {
        protected override void Configure(IObjectTypeDescriptor<IHand> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("A hand of cards, only visible to the owner");
            
            descriptor.Field(graveyard =>  graveyard.Contents)
                .Type<ListType<CardType>>();
        }
    }

    public class CardProtoDataType : Archetype<ICardProtoData>
    {
        protected override void Configure(IObjectTypeDescriptor<ICardProtoData> descriptor)
        {
            base.Configure(descriptor);
           
            descriptor.Description("Blueprint for creating card instances");
            
            descriptor.Field(data => data.Effects)
                .Ignore();

        }
    }
    public class StructureProtoDataType : Archetype<IStructureProtoData>
    {
        protected override void Configure(IObjectTypeDescriptor<IStructureProtoData> descriptor)
        {
            base.Configure(descriptor);
            descriptor.Description("Blueprint for creating a Structure instance");
            
            descriptor.Field(data => data.Effects)
                .Ignore();
        }
    }
    
    public class CreatureProtoDataType : Archetype<ICreatureProtoData>
    {
        protected override void Configure(IObjectTypeDescriptor<ICreatureProtoData> descriptor)
        {
            descriptor.Description("Blueprint for creating a Creature instance");
        }
    }

    public class UnitUnionType : UnionType
    {
        protected override void Configure(IUnionTypeDescriptor descriptor)
        {
            base.Configure(descriptor);

            descriptor.Type<StructureType>();
            descriptor.Type<CreatureType>();
        }
    }
    
    public class StructureType : Archetype<IStructure>
    {
        protected override void Configure(IObjectTypeDescriptor<IStructure> descriptor)
        {
            base.Configure(descriptor);
            descriptor.Description("An instance of a Structure");

            descriptor.Field(structure => structure.Effects)
                .Ignore();

            descriptor.Field(structure => structure.CurrentZone)
                .Type<StructureZoneType>();
            
            descriptor.Field(structure => structure.Transition)
                .Ignore();
            
            descriptor.Field(structure => structure.MoveTo(default!))
                .Ignore();

            descriptor.Field(structure => structure.Kill())
                .Ignore();
        }

        public class StructureZoneType : UnionType
        {
            protected override void Configure(IUnionTypeDescriptor descriptor)
            {
                base.Configure(descriptor);
            
                descriptor.Type<MapNodeType>();
            }
        }
    }

    public class CreatureType : Archetype<ICreature>
    {
        protected override void Configure(IObjectTypeDescriptor<ICreature> descriptor)
        {
            base.Configure(descriptor);
            descriptor.Description("An instance of a Creature");

            descriptor.Field(creature => creature.CurrentZone)
                .Type<CreatureZoneType>();
            
            descriptor.Field(creature => creature.Transition)
                .Ignore();

            descriptor.Field(creature => creature.MoveTo(default!))
                .Ignore();
        }
        
        public class CreatureZoneType : UnionType
        {
            protected override void Configure(IUnionTypeDescriptor descriptor)
            {
                base.Configure(descriptor);
            
                descriptor.Type<MapNodeType>();
                descriptor.Type<GraveyardType>();
            }
        }
    }
    
    public class CardType : Archetype<ICard>
    {
        protected override void Configure(IObjectTypeDescriptor<ICard> descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Description("A card instance");

            descriptor.Field(card => card.Effects)
                .Ignore();

            descriptor.Field(card => card.CurrentZone)
                .Type<CardZoneType>();

            descriptor.Field(card => card.Transition)
                .Ignore();
            
            descriptor.Field(card => card.MoveTo(default!))
                .Ignore();

            descriptor.Field(card => card.ReduceCost(default!))
                .Ignore();
            
            descriptor.Field("contextRulesText")
                .ResolveWith<Resolvers>(resolvers => resolvers.RulesTextWithContext(default!, default!));
            
            descriptor.Field("rulesText")
                .ResolveWith<Resolvers>(resolvers => resolvers.RulesText(default!, default!));
        }
        
        public class CardZoneType : UnionType
        {
            protected override void Configure(IUnionTypeDescriptor descriptor)
            {
                base.Configure(descriptor);
            
                descriptor.Type<DiscardPileType>();
                descriptor.Type<HandType>();
                descriptor.Type<DeckType>();
            }
        }
        
        private class Resolvers
        {
            public string RulesTextWithContext(Card card, IGameState gameState) => card.GenerateRulesText(gameState);
            public string RulesText(Card card, IProtoPool protoPool) => protoPool.GetCard(card.Name).RulesText;
        }
    }
    
    public class TargetType : Archetype<ITarget>
    {
        protected override void Configure(IObjectTypeDescriptor<ITarget> descriptor)
        {
            base.Configure(descriptor);
            
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
    
    

}