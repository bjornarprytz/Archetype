namespace Archetype.Components.Meta;

internal interface IEffectDescriptor
{
    IAffected Affected { get; }
    string Keyword { get; }
    IEnumerable<IEffectParameter> Operands { get; }
}