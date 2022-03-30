using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Archetype.Godot.Extensions;

public static class MathyExtensions
{
    public static IEnumerable<Vector3> PolySect(this Vector3[] points, int nSections)
    {
        if (points.Length < 2)
            throw new ArgumentException("At least two points required to poly-sect", nameof(points));

        var from = points.First();
        var to = points.Last();



        var length = to.DistanceTo(from);

        var stepSize = length / (nSections + 1);

        var anchors = new List<Vector3>();

        for (var i = 1; i <= nSections; i++)
        {
            anchors.Add(new Vector3(i * stepSize, from.y, from.z));
        }

        return anchors;
    }

    public static Vector3 Size(this Transform transform)
    {
        var x = transform.basis.x.Length();
        var y = transform.basis.y.Length();
        var z = transform.basis.z.Length();

        var vec1 = new Vector3(x, y, z);
        var vec2 = transform.basis.Scale;

        return vec1;
    }
    
    private static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        var q0 = p0.LinearInterpolate(p1, t);
        var q1 = p1.LinearInterpolate(p2, t);
        var q2 = p2.LinearInterpolate(p3, t);

        var r0 = q0.LinearInterpolate(q1, t);
        var r1 = q1.LinearInterpolate(q2, t);

        var s = r0.LinearInterpolate(r1, t);
        return s;
    }
}