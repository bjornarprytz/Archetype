using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.View.Atoms;
using Archetype.View.Events;

namespace Archetype.Game.Payloads.Infrastructure;

public interface IMutationObserver : IDisposable 
{
    IEnumerable<IAtomMutation> Events { get; } 
}

public class MutationObserver : IMutationObserver
{
    private readonly CompositeDisposable _disposable = new();

    private readonly List<IAtomMutation> _events = new();

    public MutationObserver(IInstanceFinder instanceFinder)
    {
        _disposable.Add(
            instanceFinder.GetAll<IGameAtom>()
                .Select(c => c.OnMutation)
                .ToObservable()
                .SelectMany(o => o)
                .Subscribe(OnMutation));
    }

    public IEnumerable<IAtomMutation> Events => _events;
    
    private void OnMutation(IAtomMutation mutation)
    {
        _events.Add(mutation);
    }
    
    public void Dispose()
    {
        _disposable?.Dispose();
    }
}