using System.Collections.Generic;

namespace Archetype
{
    public abstract class TargetInfo : ITargetSelectInfo
    {
        protected int _minTargets { get; set; } = 0;
        protected int _maxTargets { get; set; } = int.MaxValue;
        protected IList<ITarget> _options { get; set; } = new List<ITarget>();
        protected IList<ITarget> _chosenTargets { get; set; } = new List<ITarget>();

        public abstract bool IsAutomatic { get; }
        public virtual bool IsValid => _chosenTargets.Count <= _maxTargets && _chosenTargets.Count >= _minTargets;
        public virtual bool RequiresAdditionalTargets => _chosenTargets.Count < _minTargets;
        public virtual bool AllowsAdditionalTargets => _chosenTargets.Count < _maxTargets;
        public virtual int MaximumAllowedTargets => _maxTargets;
        public virtual int MinimumRequiredTargets => _minTargets;

        public virtual IList<ITarget> Options => _options;
        public virtual IEnumerable<ITarget> CurrentSelection => _chosenTargets;
        public virtual IList<ITarget> ConfirmedSelection => IsValid ? _chosenTargets : new List<ITarget>();

        public virtual bool Remove(ITarget targetToRemove) => _chosenTargets.Remove(targetToRemove);
        public virtual void ResetSelection() => _chosenTargets.Clear();
        public virtual bool Add(ITarget newTarget)
        {
            if (!AllowsAdditionalTargets) return false;
            if (!IsValidTarget(newTarget)) return false;

            _chosenTargets.Add(newTarget);

            return true;
        }

        public virtual IList<T> CastSelection<T>() where T : ITarget => _chosenTargets as IList<T>;
        public virtual bool IsValidTarget(ITarget target) => _options.Contains(target) && !_chosenTargets.Contains(target);
    }
}