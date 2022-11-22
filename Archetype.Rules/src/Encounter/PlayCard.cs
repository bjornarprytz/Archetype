namespace Archetype.Rules.Encounter;

public class PlayCardHandler
{
    public record Args(Guid CardId, IEnumerable<Guid> PaymentCardIds, IEnumerable<Guid> TargetGuids);
}