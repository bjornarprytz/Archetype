
using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;

using HotChocolate.Types;

namespace Archetype.Server.Schema;

public class PlayerType : AtomType<IPlayerFront>
{
    protected override void Configure(IObjectTypeDescriptor<IPlayerFront> descriptor)
    {
        base.Configure(descriptor);
        
        descriptor.Description("The player of the game");
    }
}

public class MapNodeType : AtomType<IMapNodeFront>
{
    protected override void Configure(IObjectTypeDescriptor<IMapNodeFront> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("A node on the map");
    }
}

public class GraveyardType : AtomType<IGraveyardFront>
{
    protected override void Configure(IObjectTypeDescriptor<IGraveyardFront> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("A graveyard that contains dead creatures");
    }
}

public class DiscardPileType : AtomType<IDiscardPileFront>
{
    protected override void Configure(IObjectTypeDescriptor<IDiscardPileFront> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("Discard pile, where spent cards go");
    }
}

public class DeckType : AtomType<IDeckFront>
{
    protected override void Configure(IObjectTypeDescriptor<IDeckFront> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("A stack of cards which replenishes the player's hand");
    }
}

public class HandType : AtomType<IHandFront>
{
    protected override void Configure(IObjectTypeDescriptor<IHandFront> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("A hand of cards, only visible to the owner");
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

public class StructureType : UnitType<IStructureFront>
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

public class CreatureType : UnitType<ICreatureFront>
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

public class CardType : AtomType<ICardFront>
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

public abstract class AtomType<T> : ObjectType<T>
    where T : IGameAtomFront
{
    protected override void Configure(IObjectTypeDescriptor<T> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Field(atom => atom.Guid);
    }
}

public abstract class UnitType<T> : AtomType<T>
    where T : IUnitFront
{
    protected override void Configure(IObjectTypeDescriptor<T> descriptor)
    {
        base.Configure(descriptor);
        
        descriptor.Field(t => t.BaseMetaData);
        descriptor.Field(t => t.MaxHealth);
        descriptor.Field(t => t.Health);
        descriptor.Field(t => t.MaxDefense);
        descriptor.Field(t => t.Defense);
    }
}