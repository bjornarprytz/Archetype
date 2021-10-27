using System;

namespace Archetype.Core
{
    public class EffectData
    {
        public Type TargetType { get; set; }
        public Type ResultType { get; set; }
        public int TargetIndex { get; set; }
        public string RulesTextFunctionName {get; set;}
        public string ResolutionFunctionName {get; set;}
    }
}