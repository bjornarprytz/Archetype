namespace Archetype
{
    public class ActionData
    {
        public ActionType Type { get; set; }
        public ActionParameter[] Params { get; set; }
        public TargetRequirementData TargetRequirements { get; set; }
    }
}
