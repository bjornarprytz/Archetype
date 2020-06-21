using System.Collections.Generic;

namespace Archetype
{
    public abstract class SelectionInfo<T> : ISelectionInfo<T>
    {
        protected int _minChoices { get; set; } = 0;
        protected int _maxChoices { get; set; } = int.MaxValue;
        protected IList<T> _options { get; set; } = new List<T>();
        protected IList<T> _choices { get; set; } = new List<T>();

        public abstract bool IsAutomatic { get; }
        public virtual bool IsValid => _choices.Count <= _maxChoices && _choices.Count >= _minChoices;
        public virtual bool RequiresAdditionalChoices => _choices.Count < _minChoices;
        public virtual bool AllowsAdditionalChoices => _choices.Count < _maxChoices;
        public virtual int MaximumAllowedChoices => _maxChoices;
        public virtual int MinimumRequiredChoices => _minChoices;

        public virtual IList<T> Options => _options;
        public virtual IEnumerable<T> CurrentSelection => _choices;
        public virtual IList<T> ConfirmedSelection => IsValid ? _choices : new List<T>();


        public SelectionInfo(IEnumerable<T> options)
        {
            _options = new List<T>(options);
        }

        public virtual bool Remove(T targetToRemove) => _choices.Remove(targetToRemove);
        public virtual void ResetSelection() => _choices.Clear();
        public virtual bool Add(T newChoice)
        {
            if (!AllowsAdditionalChoices) return false;
            if (!IsValidChoice(newChoice)) return false;

            _choices.Add(newChoice);

            return true;
        }

        public virtual bool IsValidChoice(T choice) => _options.Contains(choice) && !_choices.Contains(choice);
        public virtual IList<TCast> CastSelection<TCast>() where TCast : T => _choices as IList<TCast>;
    }
}