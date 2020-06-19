using System.Collections.Generic;

namespace Archetype
{
    public abstract class TargetInfo : ISelectionInfo<ITarget>
    {
        protected int _minTargets { get; set; } = 0;
        protected int _maxTargets { get; set; } = int.MaxValue;
        protected IList<ITarget> _options { get; set; } = new List<ITarget>();
        protected IList<ITarget> _chosenTargets { get; set; } = new List<ITarget>();

        public abstract bool IsAutomatic { get; }
        public virtual bool IsValid => _chosenTargets.Count <= _maxTargets && _chosenTargets.Count >= _minTargets;
        public virtual bool RequiresAdditionalChoices => _chosenTargets.Count < _minTargets;
        public virtual bool AllowsAdditionalChoices => _chosenTargets.Count < _maxTargets;
        public virtual int MaximumAllowedChoices => _maxTargets;
        public virtual int MinimumRequiredChoices => _minTargets;

        public virtual IList<ITarget> Options => _options;
        public virtual IEnumerable<ITarget> CurrentSelection => _chosenTargets;
        public virtual IList<ITarget> ConfirmedSelection => IsValid ? _chosenTargets : new List<ITarget>();


        public virtual bool Remove(ITarget targetToRemove) => _chosenTargets.Remove(targetToRemove);
        public virtual void ResetSelection() => _chosenTargets.Clear();
        public virtual bool Add(ITarget newTarget)
        {
            if (!AllowsAdditionalChoices) return false;
            if (!IsValidChoice(newTarget)) return false;

            _chosenTargets.Add(newTarget);

            return true;
        }

        public virtual IList<T> CastSelection<T>() where T : ITarget => _chosenTargets as IList<T>;
        public virtual bool IsValidChoice(ITarget target) => _options.Contains(target) && !_chosenTargets.Contains(target);
    }
}