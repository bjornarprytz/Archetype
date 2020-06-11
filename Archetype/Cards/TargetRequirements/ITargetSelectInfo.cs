using System.Collections.Generic;

namespace Archetype
{
    public interface ITargetSelectInfo
    {
        bool IsAutomatic { get; }
        bool RequiresAdditionalTargets { get; }
        bool AllowsAdditionalTargets { get; }
        bool IsValid { get; }
        int MaximumAllowedTargets  { get; }
        int MinimumRequiredTargets  { get; }

        IList<ITarget> Options { get; }
        IEnumerable<ITarget> CurrentSelection  { get; }
        IList<ITarget> ConfirmedSelection { get; }
        IList<T> CastSelection<T>() where T : ITarget;

        bool IsValidTarget(ITarget candidateTarget);
        bool Add(ITarget newTarget);
        bool Remove(ITarget targetToRemove);
        void ResetSelection();
    }
}
