using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Archetype
{
    public class Effect
    {
        public void Resolve(IPromptable prompt)
        {
            _resolve?.Invoke(prompt);
        }

        public Effect(Resolution resolution)
        {
            _resolve = resolution;
        }

        protected virtual Resolution _resolve { get; set; }
    }
}
