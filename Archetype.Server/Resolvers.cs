using System;
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

            descriptor.BindFieldsExplicitly();
        }
    }
    public class GameStateType : Archetype<GameState>
    {
        protected override void Configure(IObjectTypeDescriptor<GameState> descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Description("The root object of actionable game state");
            descriptor.Implements<InterfaceType<IGameState>>();

            descriptor.Field(state => state.HistoryReader)
                .Ignore();
        }
    }

    public class StructureTriggerType : Archetype<IEffect<ITriggerContext<IStructure>>>
    {
        protected override void Configure(IObjectTypeDescriptor<IEffect<ITriggerContext<IStructure>>> descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Description("I should not be here (structure trigger)"); // TODO: Where is this in the schema?

            descriptor.Field(effect => effect.ResolveContext(default!))
                .Ignore();
            
            descriptor.Field(effect => effect.ContextRulesText(default!))
                .Ignore();
            
            descriptor.Field(effect => effect.PrintedRulesText())
                .Ignore();
        }
    }
    
    public class CardEffectType : Archetype<IEffect<ICardContext>>
    {
        protected override void Configure(IObjectTypeDescriptor<IEffect<ICardContext>> descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Description("I should not be here (card effect)"); // TODO: Where is this in the schema?

            descriptor.Field(effect => effect.ResolveContext(default!))
                .Ignore();
            
            descriptor.Field(effect => effect.ContextRulesText(default!))
                .Ignore();
            
            descriptor.Field(effect => effect.PrintedRulesText())
                .Ignore();
        }
    }
    

    public class AtomType : Archetype<Atom>
    {
        protected override void Configure(IObjectTypeDescriptor<Atom> descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Description("The base class of all game object instances");
            descriptor.Implements<InterfaceType<IGameAtom>>();
        }
    }
    
    public class PlayerType : Archetype<Player>
    {
        protected override void Configure(IObjectTypeDescriptor<Player> descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Description("The player of the game");
            descriptor.Implements<InterfaceType<IPlayer>>();
        }
    }
    
    public class MapType : Archetype<Map>
    {
        protected override void Configure(IObjectTypeDescriptor<Map> descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Description("A graph of map nodes");
            descriptor.Implements<InterfaceType<IMap>>();

            descriptor.Field(map => map.Nodes)
                .Ignore(); // TODO: REmove this
        }
    }

    public class CreatureZoneType : UnionType // TODO: Possibly make these nested classes
    {
        protected override void Configure(IUnionTypeDescriptor descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Type<MapNodeType>();
            descriptor.Type<GraveyardType>();
        }
    }
    
    public class StructureZoneType : UnionType
    {
        protected override void Configure(IUnionTypeDescriptor descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Type<MapNodeType>();
        }
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
    
    public class MapNodeType : Archetype<MapNode>
    {
        protected override void Configure(IObjectTypeDescriptor<MapNode> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("A node on the map");
            descriptor.Implements<InterfaceType<IMapNode>>();
            
            descriptor.Field(node => node.Contents)
                .Type<ListType<UnitUnionType>>();

            descriptor
                .Field(node => node.CreateCreature(default!, default!))
                .Ignore();
            
            descriptor
                .Field(node => node.CreateStructure(default!, default!))
                .Ignore();

            descriptor.Field(node => node.GetGamePiece(default!))
                .Ignore();
            
            descriptor.Field(node => node.GetTypedPiece(default!))
                .Ignore();
        }
    }

    public class GraveyardType : Archetype<Graveyard>
    {
        protected override void Configure(IObjectTypeDescriptor<Graveyard> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("A graveyard that contains dead creatures");
            descriptor.Implements<InterfaceType<IGraveyard>>();
        }
    }
    
    public class DiscardPileType : Archetype<DiscardPile>
    {
        protected override void Configure(IObjectTypeDescriptor<DiscardPile> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("Discard pile, where spent cards go");
            descriptor.Implements<InterfaceType<IDiscardPile>>();
        }
    }

    public class PoolType : Archetype<ProtoPool>
    {
        protected override void Configure(IObjectTypeDescriptor<ProtoPool> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("Collection of all available sets");
            descriptor.Implements<InterfaceType<IProtoPool>>();
        }
    }

    public class SetType : Archetype<Set>
    {
        protected override void Configure(IObjectTypeDescriptor<Set> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("A set of proto data which share some themes");
            descriptor.Implements<InterfaceType<ISet>>();
        }
    }

    public class DeckType : Archetype<Deck>
    {
        protected override void Configure(IObjectTypeDescriptor<Deck> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("A stack of cards which replenishes the player's hand");
            descriptor.Implements<InterfaceType<IDeck>>();
        }
    }
    
    public class HandType : Archetype<Hand>
    {
        protected override void Configure(IObjectTypeDescriptor<Hand> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("A hand of cards, only visible to the owner");
            descriptor.Implements<InterfaceType<IHand>>();
        }
    }

    public class CardProtoDataType : Archetype<CardProtoData>
    {
        protected override void Configure(IObjectTypeDescriptor<CardProtoData> descriptor)
        {
            base.Configure(descriptor);
           
            descriptor.Description("Blueprint for creating card instances");
            descriptor.Implements<InterfaceType<ICardProtoData>>();
            
            descriptor.Field(data => data.Effects)
                .Ignore();

        }
    }
    public class StructureProtoDataType : Archetype<StructureProtoData>
    {
        protected override void Configure(IObjectTypeDescriptor<StructureProtoData> descriptor)
        {
            base.Configure(descriptor);
            descriptor.Description("Blueprint for creating a Structure instance");
            descriptor.Implements<InterfaceType<IStructureProtoData>>();
            
            descriptor.Field(data => data.Effects)
                .Ignore();
        }
    }
    
    public class CreatureProtoDataType : Archetype<CreatureProtoData>
    {
        protected override void Configure(IObjectTypeDescriptor<CreatureProtoData> descriptor)
        {
            descriptor.Description("Blueprint for creating a Creature instance");
            descriptor.Implements<InterfaceType<ICreatureProtoData>>();
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
    
    public class StructureType : Archetype<Structure>
    {
        protected override void Configure(IObjectTypeDescriptor<Structure> descriptor)
        {
            base.Configure(descriptor);
            descriptor.Description("An instance of a Structure");
            descriptor.Implements<InterfaceType<IStructure>>();

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
    }

    public class CreatureType : Archetype<Creature>
    {
        protected override void Configure(IObjectTypeDescriptor<Creature> descriptor)
        {
            base.Configure(descriptor);
            descriptor.Description("An instance of a Creature");
            descriptor.Implements<InterfaceType<ICreature>>();

            descriptor.Field(creature => creature.CurrentZone)
                .Type<CreatureZoneType>();
            
            descriptor.Field(creature => creature.Transition)
                .Ignore();

            descriptor.Field(creature => creature.MoveTo(default!))
                .Ignore();
        }
    }
    
    public class CardType : Archetype<Card>
    {
        protected override void Configure(IObjectTypeDescriptor<Card> descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Description("A card instance");
            descriptor.Implements<InterfaceType<ICard>>();

            descriptor.Field(card => card.Effects)
                .Ignore();

            descriptor.Field(card => card.CurrentZone)
                .Type<CardZoneType>();

            descriptor.Field(card => card.Transition)
                .Ignore();
            
            descriptor.Field(card => card.MoveTo(default!))
                .Ignore();
            
            descriptor.Field(card => card.GenerateRulesText(default!))
                .Ignore();
            
            descriptor.Field("contextRulesText")
                .ResolveWith<Resolvers>(resolvers => resolvers.RulesTextWithContext(default!, default!));
            
            descriptor.Field("rulesText")
                .ResolveWith<Resolvers>(resolvers => resolvers.RulesText(default!, default!));

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