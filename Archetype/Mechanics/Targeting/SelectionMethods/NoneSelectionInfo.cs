using System.Linq;

namespace Archetype
{
    public class NoneSelectionInfo<T> : SelectionInfo<T>
    {
        public override bool IsAutomatic => true;

        public NoneSelectionInfo() : base(Enumerable.Empty<T>())
        {

        }
    }
}
