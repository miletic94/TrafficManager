using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.TestTools;

public class AStarTest
{
    private AStarNode startNode;
    private AStarNode endNode;

    private SplineContainer container;

    [SetUp]
    public void SetUp()
    {
        container = new SplineContainer();
        Spline spline0, spline1, spline2, spline3;

        BezierKnot knot0 = new BezierKnot(new float3(0, 0, 0), new float3(0, 0, -5), new float3(0, 0, 5), new quaternion(0, 1, 0, 0));
        BezierKnot knot1 = new BezierKnot(new float3(0, 0, -50), new float3(0, 0, -16), new float3(0, 0, 16), new quaternion(0, 1, 0, 0));
        BezierKnot knot2 = new BezierKnot(new float3(0, 0, -100), new float3(0, 0, -16), new float3(0, 0, 16), new quaternion(0, 1, 0, 0));

        var knots0 = new BezierKnot[3] { knot0, knot1, knot2 };
        spline0 = new Spline(knots0);

        BezierKnot knot3 = new BezierKnot(new float3(-50, 0, -50), new float3(0, 0, -16), new float3(0, 0, 16), new quaternion(0, 0.70710677f, 0, 0.70710677f));
        BezierKnot knot4 = new BezierKnot(new float3(0, 0, -50), new float3(0, 0, -16), new float3(0, 0, 16), new quaternion(0, 0.70710677f, 0, 0.70710677f));
        BezierKnot knot5 = new BezierKnot(new float3(50, 0, -50), new float3(0, 0, -16), new float3(0, 0, 16), new quaternion(0, 0.70710677f, 0, 0.70710677f));

        var knots1 = new BezierKnot[3] { knot3, knot4, knot5 };
        spline1 = new Spline(knots1);

        BezierKnot knot6 = new BezierKnot(new float3(50, 0, 0), new float3(0, 0, -16), new float3(0, 0, 16), new quaternion(0, 1, 0, 0));
        BezierKnot knot7 = new BezierKnot(new float3(50, 0, -50), new float3(0, 0, -16), new float3(0, 0, 16), new quaternion(0, 1, 0, 0));
        BezierKnot knot8 = new BezierKnot(new float3(50, 0, -100), new float3(0, 0, -16), new float3(0, 0, 16), new quaternion(0, 1, 0, 0));

        var knots2 = new BezierKnot[3] { knot6, knot7, knot8 };
        spline2 = new Spline(knots2);

        BezierKnot knot9 = new BezierKnot(new float3(-50, 0, -100), new float3(0, 0, -16), new float3(0, 0, 16), new quaternion(0, 0.70710677f, 0, 0.70710677f));
        BezierKnot knot10 = new BezierKnot(new float3(0, 0, -100), new float3(0, 0, -16), new float3(0, 0, 16), new quaternion(0, 0.70710677f, 0, 0.70710677f));
        BezierKnot knot11 = new BezierKnot(new float3(50, 0, -100), new float3(0, 0, -16), new float3(0, 0, 16), new quaternion(0, 0.70710677f, 0, 0.70710677f));
        BezierKnot knot12 = new BezierKnot(new float3(100, 0, -100), new float3(0, 0, -16), new float3(0, 0, 16), new quaternion(0, 0.70710677f, 0, 0.70710677f));

        var knots3 = new BezierKnot[4] { knot9, knot10, knot11, knot12 };
        spline3 = new Spline(knots3);

        container.AddSpline(spline0);
        container.AddSpline(spline1);
        container.AddSpline(spline2);
        container.AddSpline(spline3);
    }
}
