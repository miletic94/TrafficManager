using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.TestTools;

public class AStarTest
{
    private GameObject gameObject = new GameObject();
    private SplineContainer container;

    [SetUp]
    public void SetUp()
    {
        container = gameObject.AddComponent<SplineContainer>();

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

        container.Splines = new Spline[4] { spline0, spline1, spline2, spline3 };

        container.KnotLinkCollection.Link(new SplineKnotIndex(0, 1), new SplineKnotIndex(1, 1));
        container.KnotLinkCollection.Link(new SplineKnotIndex(0, 2), new SplineKnotIndex(3, 1));
        container.KnotLinkCollection.Link(new SplineKnotIndex(1, 2), new SplineKnotIndex(2, 1));
        container.KnotLinkCollection.Link(new SplineKnotIndex(2, 2), new SplineKnotIndex(3, 2));

    }

    [Test]
    public void AStarNode_Instantiate()
    {
        SplineKnotIndex splineKnotIndex = new SplineKnotIndex(1, 1);
        AStarNode node = new AStarNode(container, splineKnotIndex);

        Assert.IsInstanceOf<AStarNode>(node);
        Assert.AreEqual(new SplineKnotIndex(0, 1), node.KnotLinksSet.First());
        Assert.AreEqual(node.gCost, float.PositiveInfinity);
        Assert.AreEqual(node.hCost, float.NegativeInfinity);
        Assert.AreEqual(node.fCost, float.NaN);
    }
}
