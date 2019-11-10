using System;

namespace Archetype
{
    public class ChooseCards : ActionPrompt
    {
        public override Type RequiredType => typeof(Card);

        public ChooseCards(int x)
            : base (x)
        {

        }

        public ChooseCards(int min, int max)
            : base(min, max)
        {

        }
    }
}
