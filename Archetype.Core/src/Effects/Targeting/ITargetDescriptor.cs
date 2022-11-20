namespace Archetype.Core.Effects;

public interface ITargetDescriptor
{
    Type TargetType { get; } // TODO: Possibly make this a domain type instead of a CLR type (e.g. Artifact, Player, etc.)
}