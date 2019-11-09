using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public abstract class UserPrompt
    {
        public int MaxChoices { get; protected set; }
        public int MinChoices { get; protected set; }

        public Type RequiredType
        {
            get { return _requiredType; }
            private set
            {
                if (!value.IsSubclassOf(_typeRestriction)) throw new Exception($"Supplied type should be subclass of {_typeRestriction}");

                _requiredType = value;
            }
        }
        private Type _requiredType;
        protected virtual Type _typeRestriction => typeof(object);

        public UserPrompt(int x, Type requiredType)
        {
            RequiredType = requiredType;
            MaxChoices = MinChoices = x;
        }
        public UserPrompt(int min, int max, Type requiredType)
        {
            RequiredType = requiredType;
            MaxChoices = max;
            MinChoices = min;
        }

        public virtual bool MeetsRequirements(object choice)
        {
            if (choice.GetType() != RequiredType) return false;

            return true;
        }
    }
}
