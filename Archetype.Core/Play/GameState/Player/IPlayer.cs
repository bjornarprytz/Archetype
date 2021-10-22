using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archetype.Core
{
    public interface IPlayer
    {
        IHand Hand { get; }
        IDiscardPile DiscardPile { get; }
        int Resources { get; set; }
    }
}
