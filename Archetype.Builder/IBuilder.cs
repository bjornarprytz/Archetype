using System;

namespace Archetype.Builder
{
    public interface IBuilder<out T>
    {
        T Build();
    }

}
