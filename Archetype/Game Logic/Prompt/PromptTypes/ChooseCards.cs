using System;

namespace Archetype
{
    public class ChooseCards : ActionPrompt
    {

        protected override Type _typeRestriction => typeof(Card);

        public ChooseCards(int x)
            : base (x, typeof(Card))
        {

        }

        public ChooseCards(int min, int max)
            : base(min, max, typeof(Card))
        {

        }
    }
}
