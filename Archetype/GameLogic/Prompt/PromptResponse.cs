
namespace Archetype
{
    public class PromptResponse
    {
        public bool Aborted { get; protected set; }

        public PromptResponse(bool aborted = false) 
        {
            Aborted = aborted;
        }
    }
}
