using System.Collections.Generic;

namespace Archetype
{
    public interface ISelectionInfo<T>
    {
        bool IsAutomatic { get; }
        bool RequiresAdditionalChoices { get; }
        bool AllowsAdditionalChoices { get; }
        bool IsValid { get; }
        int MaximumAllowedChoices  { get; }
        int MinimumRequiredChoices  { get; }

        IList<T> Options { get; }
        IEnumerable<T> CurrentSelection  { get; }
        IList<T> ConfirmedSelection { get; }
        IList<TCast> CastSelection<TCast>() where TCast : T;

        bool IsValidChoice(T candidate);
        bool Add(T newChoice);
        bool Remove(T choiceToRemove);
        void ResetSelection();
    }
}
