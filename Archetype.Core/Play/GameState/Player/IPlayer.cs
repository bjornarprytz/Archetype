﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archetype.Core
{
    public interface IPlayer
    {
        public IHand Hand { get; }
        public IDiscardPile DiscardPile { get; }
        
        public int Resources { get; set; }
    }
}