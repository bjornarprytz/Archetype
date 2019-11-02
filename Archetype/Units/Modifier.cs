using System;

namespace Archetype
{
    public class Modifier
    {
        public string Keyword => _keyword.Keyword;
        private IKeyword _keyword;
        public int Value { get; private set; }


        public Modifier(int value, IKeyword keyword)
        {
            Value = value;
            _keyword = keyword;
        }
    }
}