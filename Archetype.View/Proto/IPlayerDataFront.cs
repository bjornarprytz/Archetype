namespace Archetype.View.Proto;

public interface IPlayerDataFront
{
    int StartingResources { get; } 
    int MaxHandSize { get; }
    int MinDeckSize { get; }
        
    IStructureProtoDataFront Headquarters { get; }
    IEnumerable<IStructureProtoDataFront> StructurePool { get; }
    IEnumerable<ICardProtoDataFront> CardPool { get; }
    IEnumerable<ICardProtoDataFront> DeckList { get; }
}