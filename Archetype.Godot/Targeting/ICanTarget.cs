using Godot;

namespace Archetype.Godot.Targeting
{
    public interface ICanTarget
    {
        ITargetingArrow TargetingArrow { get; }
    }
    
    public interface ITargetingArrow
    {
        void Activate();
        void Deactivate();
        void SetAnchor(Vector2 anchorPos);
        void ChangePosition(Vector2 newPos);

        Node GetTarget();
    }
}