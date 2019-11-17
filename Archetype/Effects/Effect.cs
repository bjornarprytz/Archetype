using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public abstract class Effect
    {
        public delegate void Resolution(RequiredAction prompt);
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
        public List<Unit> Targets { get; set; }
        public Unit Source { get; set; }


        public void Resolve(RequiredAction prompt)
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

        public Effect(Unit source, List<Unit> targets=null)
        {
            Source = source;
            Targets = targets ?? new List<Unit>();
        }

        protected abstract Resolution _resolve { get; }
        protected virtual Cancellation _cancel => delegate { /* What to do when cancelling an effect? */ };
        protected virtual CleanUp _cleanup => delegate { };

    }
}
