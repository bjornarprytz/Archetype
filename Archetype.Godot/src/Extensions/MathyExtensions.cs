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


    public static Vector3 Right(this Transform transform, float weight=1f) => transform.basis.x * weight;
    public static Vector3 Left(this Transform transform, float weight=1f) => -transform.Right(weight);
    public static Vector3 Up(this Transform transform, float weight=1f) => transform.basis.y * weight;
    public static Vector3 Down(this Transform transform, float weight=1f) => -transform.Up(weight);
    public static Vector3 Backward(this Transform transform, float weight=1f) => transform.basis.z * weight;
    public static Vector3 Forward(this Transform transform, float weight=1f) => -transform.Backward(weight);
    
    
    public static Vector3 Size(this Transform transform)
    {
        var x = transform.basis.x.Length();
        var y = transform.basis.y.Length();
        var z = transform.basis.z.Length();

        var vec1 = new Vector3(x, y, z);
        var vec2 = transform.basis.Scale;

        return vec1;
    }
    
    public static IEnumerable<Vector3> BezierPoints(this BezierParameters p, int nPoints)
    {
        if (nPoints <= 0)
            throw new ArgumentException("nPoints must be positive", nameof(nPoints));
			
        var dt = 1f / nPoints;

        var t = 0f;

        var points = new List<Vector3>();
			
        while (t < 1f)
        {
            points.Add(p.BezierPoint(t));
				
            t += dt;
        }
			
        points.Add(p.BezierPoint(1f));

        return points;
    }
    
    public static Vector3 BezierPoint(this BezierParameters p, float t)
    {
        var q0 = p.Origin.LinearInterpolate(p.EntryNormal, t);
        var q1 = p.EntryNormal.LinearInterpolate(p.ExitNormal, t);
        var q2 = p.ExitNormal.LinearInterpolate(p.Target, t);

        var r0 = q0.LinearInterpolate(q1, t);
        var r1 = q1.LinearInterpolate(q2, t);

        var s = r0.LinearInterpolate(r1, t);
        return s;
    }
    
    public struct BezierParameters
    {
        public BezierParameters(Vector3 origin, Vector3 entryNormal, Vector3 exitNormal, Vector3 target)
        {
            Origin = origin;
            EntryNormal = entryNormal;
            ExitNormal = exitNormal;
            Target = target;
        }

        public Vector3 Origin { get; }
        public Vector3 EntryNormal { get; }
        public Vector3 ExitNormal { get; }
        public Vector3 Target { get; }
    }
}