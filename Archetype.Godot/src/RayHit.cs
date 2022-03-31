using System.ComponentModel;
using Godot;
using Godot.Collections;

namespace Archetype.Godot.StateMachine;

public class RayHit
{
    private readonly Dictionary _dictionary;

    public RayHit(Dictionary rayHitCollection)
    {
        Hit = rayHitCollection.Count > 0;

        _dictionary = rayHitCollection;
    }

    [Description("True if the ray hit something")]
    public bool Hit { get; }

    [Description("The colliding object.")] 
    public object Collider => _dictionary["collider"];

    [Description("The colliding object's ID.")]
    public int ColliderId => (int)_dictionary["collider_id"];

    [Description("The shape index of the colliding shape.")]
    public int Shape => (int)_dictionary["shape"];

    [Description("The object's surface normal at the intersection point.")]
    public Vector3 Normal => (Vector3)_dictionary["normal"];

    [Description("The intersection point.")]
    public Vector3 Position => (Vector3)_dictionary["position"];

    [Description("The intersecting object's RID. ")]
    public RID Rid => (RID)_dictionary["rid"];


    public T GetCollidingNode<T>() where T : Node
    {
        return Hit
            ? Collider as T
            : null;
    }
}