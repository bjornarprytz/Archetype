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
        
        descriptor.IsOfType((context, result) => result is IMapNodeFront);
    }
}

public class GraveyardType : AtomType<IGraveyardFront>
{
    protected override void Configure(IObjectTypeDescriptor<IGraveyardFront> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("A graveyard that contains dead creatures");

        descriptor.IsOfType((context, result) => result is IGraveyardFront);
    }
}

public class DiscardPileType : AtomType<IDiscardPileFront>
{
    protected override void Configure(IObjectTypeDescriptor<IDiscardPileFront> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("Discard pile, where spent cards go");
        
        descriptor.IsOfType((context, result) => result is IDiscardPileFront);
    }
}

public class DeckType : AtomType<IDeckFront>
{
    protected override void Configure(IObjectTypeDescriptor<IDeckFront> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("A stack of cards which replenishes the player's hand");
        
        descriptor.IsOfType((context, result) => result is IDeckFront);
    }
}

public class HandType : AtomType<IHandFront>
{
    protected override void Configure(IObjectTypeDescriptor<IHandFront> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("A hand of cards, only visible to the owner");
        
        descriptor.IsOfType((context, result) => result is IHandFront);
    }
}

public class StructureType : UnitType<IStructureFront, StructureType.StructureZoneUnion>
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

public class CreatureType : UnitType<ICreatureFront, CreatureType.CreatureZoneUnion>
{
    protected override void Configure(IObjectTypeDescriptor<ICreatureFront> descriptor)
    {
        base.Configure(descriptor);
        descriptor.Description("An instance of a Creature");
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

public class CardType : PieceType<ICardFront, CardType.CardZoneUnion>
{
    protected override void Configure(IObjectTypeDescriptor<ICardFront> descriptor)
    {
        base.Configure(descriptor);
        
        descriptor.Description("A card instance");
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

public abstract class UnitType<T, TZone> : PieceType<T, TZone>
    where T : IUnitFront
    where TZone : UnionType
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

public abstract class PieceType<T, TZone> : AtomType<T>
    where T : IPieceFront
    where TZone : UnionType
{
    protected override void Configure(IObjectTypeDescriptor<T> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Field(piece => piece.Name);
        descriptor.Field(piece => piece.CurrentZone)
            .Type<TZone>();
    }
}


public abstract class AtomType<T> : ObjectType<T>
    where T : IGameAtomFront
{
    protected override void Configure(IObjectTypeDescriptor<T> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Field(atom => atom.Guid);
        descriptor.Field(atom => atom.Owner);

        descriptor.IsOfType((context, result) => result is T);
    }
    
}
public class AtomUnion : UnionType
{
    protected override void Configure(IUnionTypeDescriptor descriptor)
    {
        base.Configure(descriptor);
        
        descriptor.Type<CardType>();
        descriptor.Type<CreatureType>();
        descriptor.Type<DeckType>();
        descriptor.Type<DiscardPileType>();
        descriptor.Type<GraveyardType>();
        descriptor.Type<HandType>();
        descriptor.Type<MapNodeType>();
        descriptor.Type<PlayerType>();
        descriptor.Type<StructureType>();
    }
}


