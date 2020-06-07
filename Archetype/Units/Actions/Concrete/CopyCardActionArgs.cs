namespace Archetype
{
    public class CopyCardActionArgs : ActionInfo
    {
        public Unit NewOwner { get; set; }
        public Zone<Card> TargetZone { get; set; }

        public CopyCardActionArgs(Unit source, Card target, Unit newOwner, Zone<Card> targetZone) :base(source, target)
        {
            NewOwner = newOwner;
            TargetZone = targetZone;
        }

        protected override void Resolve()
        {
            var copy = (Target as Card).MakeCopy(NewOwner);

            copy.MoveTo(TargetZone);
        }
    }
}
