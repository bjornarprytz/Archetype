using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Archetype.Godot.Extensions;

public static class MathyExtensions
{
    public static IEnumerable<Vector2> PolySect(this Vector2[] points, int nSections)
    {
        if (points.Length < 2)
            throw new ArgumentException("At least two points required to poly-sect", nameof(points));

        var from = points.First();
        var to = points.Last();
        
        var length = Math.Abs(to.x - from.x);

        var stepSize = length / (nSections + 1);

        var anchors = new List<Vector2>();

        for (var i = 1; i <= nSections; i++)
        {
            anchors.Add(new Vector2(i * stepSize, from.y));
        }

        return anchors;
    }
}