using System;

namespace Archetype.CardBuilder
{
    public interface IBuilder<out T>
    {
        T Build();
    }

}
