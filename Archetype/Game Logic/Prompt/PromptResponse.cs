
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class PromptResponse
    {
        public List<Unit> Choices { get; set; }
        public bool Aborted { get; protected set; }



        public static PromptResponse Choose(IEnumerable<Unit> choices)
        {
            return new PromptResponse() { Choices = choices.ToList(), Aborted = false };
        }

        public static PromptResponse Abort()
        {
            return new PromptResponse() { Aborted = true };
        }
    }
}
