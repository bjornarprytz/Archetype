using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public interface IPromptable
    {
       void Prompt(ActionPrompt actionPrompt);
       PromptResponse PromptImmediate(ActionPrompt actionPrompt);
    }
}
