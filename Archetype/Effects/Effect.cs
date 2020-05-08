using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Archetype
{
    public abstract class Effect<T> : Effect, IOwned<Unit> where T : GamePiece
    {
        public override Type TargetType => typeof(T);

        public Effect(EffectArgs args) : base(args) { }
    }

    public abstract class Effect
    {
        public delegate void Resolution(IPromptable prompt);
        public delegate void Cancellation();
        public delegate void CleanUp();

        public delegate void ResolvedEffect(Effect effect);
        public delegate void CancelledEffect(Effect effect);
        public delegate void CleanedUpEffect(Effect effect);

        public event ResolvedEffect OnResolve;
        public event CancelledEffect OnCancel;
        public event CleanedUpEffect OnCleanup;

        public bool HasSource { get { return Source != null; } }
        public bool HasTargets { get { return Targets != null && Targets.Count > 0; } }
        
        public abstract Type TargetType { get; } 
        public List<GamePiece> Targets { get; set; }
        public Unit Source { get; set; }
        public Unit Owner => Source;


        public void Resolve(IPromptable prompt)
        {
            _resolve?.Invoke(prompt);
            OnResolve?.Invoke(this);

            _cleanup?.Invoke();
            OnCleanup?.Invoke(this);
        }
        public void Cancel()
        {
            _cancel?.Invoke();
            OnCancel?.Invoke(this);

            _cleanup?.Invoke();
            OnCleanup?.Invoke(this);
        }

        public Effect(EffectArgs args)
        {
            Source = args.Source;
            Targets = args.Targets;
        }

        protected abstract Resolution _resolve { get; }
        protected virtual Cancellation _cancel => delegate { /* What to do when cancelling an effect? */ };
        protected virtual CleanUp _cleanup => delegate { };

    }
}
