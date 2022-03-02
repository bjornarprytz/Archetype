using System;
using System.Collections.Generic;

namespace Archetype.Godot.Extensions
{
    public static class LocalReactiveExtensions
    {
        public static T DisposeWith<T>(this T disposable, ICollection<IDisposable> disposables)
            where T : IDisposable
        {
            disposables.Add(disposable);

            return disposable;
        }
    }
}