using System;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Proto;
using Archetype.View;
using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;
using Archetype.View.Infrastructure;
using Archetype.View.Proto;
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
    public class GameStateType : Archetype<IGameStateFront>
    {
        protected override void Configure(IObjectTypeDescriptor<IGameStateFront> descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Description("The root object of actionable game state");
        }
    }

    public class AtomType : Archetype<IGameAtomFront>
    {
        protected override void Configure(IObjectTypeDescriptor<IGameAtomFront> descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Description("The base class of all game object instances");
        }
    }
    
    public class PlayerType : Archetype<IPlayerFront>
    {
        protected override void Configure(IObjectTypeDescriptor<IPlayerFront> descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Description("The player of the game");
        }
    }
    
    public class MapType : Archetype<IMapFront>
    {
        protected override void Configure(IObjectTypeDescriptor<IMapFront> descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Description("A graph of map nodes");
        }
    }

    public class MapNodeType : Archetype<IMapNodeFront>
    {
        protected override void Configure(IObjectTypeDescriptor<IMapNodeFront> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("A node on the map");
        }
    }

    public class GraveyardType : Archetype<IGraveyardFront>
    {
        protected override void Configure(IObjectTypeDescriptor<IGraveyardFront> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("A graveyard that contains dead creatures");
        }
    }
    
    public class DiscardPileType : Archetype<IDiscardPileFront>
    {
        protected override void Configure(IObjectTypeDescriptor<IDiscardPileFront> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("Discard pile, where spent cards go");
        }
    }

    public class PoolType : Archetype<IProtoPoolFront>
    {
        protected override void Configure(IObjectTypeDescriptor<IProtoPoolFront> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("Collection of all available sets");
        }
    }

    public class SetType : Archetype<ISetFront>
    {
        protected override void Configure(IObjectTypeDescriptor<ISetFront> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("A set of proto data which share some themes");
        }
    }

    public class DeckType : Archetype<IDeckFront>
    {
        protected override void Configure(IObjectTypeDescriptor<IDeckFront> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("A stack of cards which replenishes the player's hand");
        }
    }
    
    public class HandType : Archetype<IHandFront>
    {
        protected override void Configure(IObjectTypeDescriptor<IHandFront> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Description("A hand of cards, only visible to the owner");
        }
    }

    public class CardProtoDataType : Archetype<ICardProtoDataFront>
    {
        protected override void Configure(IObjectTypeDescriptor<ICardProtoDataFront> descriptor)
        {
            base.Configure(descriptor);
           
            descriptor.Description("Blueprint for creating card instances");
        }
    }
    public class StructureProtoDataType : Archetype<IStructureProtoDataFront>
    {
        protected override void Configure(IObjectTypeDescriptor<IStructureProtoDataFront> descriptor)
        {
            base.Configure(descriptor);
            descriptor.Description("Blueprint for creating a Structure instance");
        }
    }
    
    public class CreatureProtoDataType : Archetype<ICreatureProtoDataFront>
    {
        protected override void Configure(IObjectTypeDescriptor<ICreatureProtoDataFront> descriptor)
        {
            descriptor.Description("Blueprint for creating a Creature instance");
        }
    }

    public class UnitUnion : UnionType
    {
        protected override void Configure(IUnionTypeDescriptor descriptor)
        {
            base.Configure(descriptor);

            descriptor.Type<StructureType>();
            descriptor.Type<CreatureType>();
        }
    }
    
    public class StructureType : Archetype<IStructureFront>
    {
        protected override void Configure(IObjectTypeDescriptor<IStructureFront> descriptor)
        {
            base.Configure(descriptor);
            descriptor.Description("An instance of a Structure");
        }

        public class StructureZoneUnion : UnionType
        {
            protected override void Configure(IUnionTypeDescriptor descriptor)
            {
                base.Configure(descriptor);
            
                descriptor.Type<MapNodeType>();
            }
        }
    }

    public class CreatureType : Archetype<ICreatureFront>
    {
        protected override void Configure(IObjectTypeDescriptor<ICreatureFront> descriptor)
        {
            base.Configure(descriptor);
            descriptor.Description("An instance of a Creature");

            descriptor.Field(creature => creature.CurrentZone)
                .Type<CreatureZoneUnion>();
        }
        
        public class CreatureZoneUnion : UnionType
        {
            protected override void Configure(IUnionTypeDescriptor descriptor)
            {
                base.Configure(descriptor);
            
                descriptor.Type<MapNodeType>();
                descriptor.Type<GraveyardType>();
            }
        }
    }
    
    public class CardType : Archetype<ICardFront>
    {
        protected override void Configure(IObjectTypeDescriptor<ICardFront> descriptor)
        {
            base.Configure(descriptor);
            
            descriptor.Description("A card instance");

            descriptor.Field(card => card.CurrentZone)
                .Type<CardZoneUnion>();
        }
        
        public class CardZoneUnion : UnionType
        {
            protected override void Configure(IUnionTypeDescriptor descriptor)
            {
                base.Configure(descriptor);
            
                descriptor.Type<DiscardPileType>();
                descriptor.Type<HandType>();
                descriptor.Type<DeckType>();
            }
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