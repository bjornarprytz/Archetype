using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class Card : GamePiece
    {
        public delegate void ZoneChange(Zone from, Zone to);
        public delegate void Resolved();
        public event ZoneChange OnZoneChanged;
        public event Resolved OnResolved;

        public string Name { get; set; }
        public string RulesText { get; set; }
        public EffectSpan Effects { get; set; }

        
        public Zone CurrentZone
        {
            get { return currZone; }
            private set
            {
                Zone previousZone = currZone;
                currZone = value;
                OnZoneChanged?.Invoke(previousZone, currZone);
            }
        }
        private Zone currZone;

        public Card(string name, Zone zone=null) : base()
        {
            Name = name;
            CurrentZone = zone;
        }

        public void MoveTo(Zone newZone)
        {
            if (CurrentZone == newZone) return;

            if (CurrentZone != null) CurrentZone.Out(this);
            if (newZone != null) newZone.Into(this);

            CurrentZone = newZone;
        }

        public virtual void Resolve() { OnResolved?.Invoke(); }
    }
}
