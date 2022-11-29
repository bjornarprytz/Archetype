using Archetype.Components.Builders;

namespace Archetype.Components;

public class BuilderFactory
{
    public static ISpellBuilder CreateSpellBuilder()
    {
        return new SpellBuilder();
    }
}