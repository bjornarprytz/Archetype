using Godot;

namespace Archetype.Godot.Targeting
{
    public interface ITargetable
    {
        Area2D TargetNode { get; } // TODO: Develop this targeting payload to be as useful as possible
    }
}