
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    class DamageParameterData : ActionParameterData
    {
        public int Strength { get; set; }

        protected override ActionInfo GetActionInfo(Unit source, ITarget target) => new DamageActionArgs(source, target as Unit, Strength);
    }
}
