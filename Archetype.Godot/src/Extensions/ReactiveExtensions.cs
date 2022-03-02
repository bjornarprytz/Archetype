using System;
using System.Reactive.Disposables;

namespace Archetype.Godot.Extensions;

public static class ReactiveExtensions
{
    public static void AddTo(this IDisposable disposable, CompositeDisposable composite)
    {
        composite.Add(disposable);
    }
}