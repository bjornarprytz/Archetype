using System;

namespace Archetype
{
    public class ChooseCardPrompt : UserPrompt
    {

        protected override Type _typeRestriction => typeof(Card);

        public ChooseCardPrompt(int x)
            : base (x, typeof(Card))
        {

        }

        public ChooseCardPrompt(int min, int max, Type requiredType)
            : base(min, max, typeof(Card))
        {

        }
    }
}
