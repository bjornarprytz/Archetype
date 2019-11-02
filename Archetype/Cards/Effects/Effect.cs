using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public abstract class Effect
    {
        public delegate void Resolution(DecisionPrompt prompt);
        public delegate void Cancellation();

        public delegate void ResolvedEffect(Effect effect);
        public delegate void CancelledEffect(Effect effect);

        public event ResolvedEffect OnResolve;
        public event CancelledEffect OnCancel;

        public void Resolve(DecisionPrompt prompt)
        {
            _resolve?.Invoke(prompt);
            OnResolve?.Invoke(this);
        }
        public void Cancel()
        {
            _cancel?.Invoke();
            OnCancel?.Invoke(this);
        }

        public bool HasSource { get { return Source != null; } }
        public bool HasTargets { get { return Targets.Count > 0; } }
        public List<Unit> Targets { get; set; }
        public Unit Source { get; set; }

        public Effect(Unit source)
        {
            Source = source;
            Targets = new List<Unit>();
        }

        public EffectSpan Whence { get; set; }

        protected abstract Resolution _resolve { get; }
        protected virtual Cancellation _cancel => delegate { /* What to do when cancelling an effect? */ };
    }
}
