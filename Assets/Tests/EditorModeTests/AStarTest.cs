using System;
using System.Linq;
using NUnit.Framework;
using Moq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;
public class AStarTest
{
    private GameObject gameObject = new GameObject();
    private SplineContainer container;

    [SetUp]
    public void SetUp()
    {

        // TODO Check SplineUtility.SetLinkedKnotPosition for setting up spline container
        // Check SplineUtility.AreKnotLinked
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
        Assert.AreEqual(node.SplineContainer, container);
    }

    // TODO: Put in Utils for now
    // [Test]
    // public void AreNeighbors_Neighbors()
    // {
    //     bool areNs = AStarNode.AreSKINeighbors(new SplineKnotIndex(2, 3), new SplineKnotIndex(2, 2));
    //     Assert.IsTrue(areNs);
    // }

    // [Test]
    // public void AreNeighbors_NonNeighbors()
    // {
    //     bool areNs = AStarNode.AreSKINeighbors(new SplineKnotIndex(2, 3), new SplineKnotIndex(2, 5));
    //     Assert.IsFalse(areNs);
    // }

    [Test]
    public void FindSKIConnectionToNode_Success()
    {
        AStarNode currentNode = new AStarNode(container, new SplineKnotIndex(2, 2));
        AStarNode otherNode = new AStarNode(container, new SplineKnotIndex(3, 3));

        currentNode.FindSKIConnectionToNode(otherNode, out SplineKnotIndex currentSKI, out SplineKnotIndex otherSKI);

        Assert.AreEqual(new SplineKnotIndex(3, 2), currentSKI);
        Assert.AreEqual(new SplineKnotIndex(3, 3), otherSKI);
    }

    [Test]
    public void FindSKIConnectionToNode_SameNode()
    {
        AStarNode currentNode = new AStarNode(container, new SplineKnotIndex(2, 2));
        AStarNode otherNode = currentNode;

        currentNode.FindSKIConnectionToNode(otherNode, out SplineKnotIndex currentSKI, out SplineKnotIndex otherSKI);

        Assert.AreEqual(currentSKI, otherSKI);
        Assert.AreEqual(new SplineKnotIndex(2, 2), currentSKI);
        Assert.AreEqual(new SplineKnotIndex(2, 2), otherSKI);
    }

    [Test]
    public void FindSKIConnectionToNode_Fail()
    {
        AStarNode currentNode = new AStarNode(container, new SplineKnotIndex(2, 2));
        AStarNode otherNode = new AStarNode(container, new SplineKnotIndex(1, 0));

        Assert.Throws<Exception>(() =>
        {
            currentNode.FindSKIConnectionToNode(otherNode, out SplineKnotIndex currentSKI, out SplineKnotIndex otherSKI);
        });
    }

    [Test]
    public void GetDistanceBySKIConnections_Connected()
    {
        SplineKnotIndex fromSKI = new SplineKnotIndex(2, 0);
        SplineKnotIndex toSKI = new SplineKnotIndex(2, 1);

        float distance = Utils.GetDistanceBySKIConnections(container, fromSKI, toSKI);

        Assert.AreEqual(50, distance);
    }

    [Test]
    public void GetDistanceBySKIConnections_Unordered()
    {
        SplineKnotIndex fromSKI = new SplineKnotIndex(2, 1);
        SplineKnotIndex toSKI = new SplineKnotIndex(2, 0);

        Assert.Throws<ArgumentException>(() => Utils.GetDistanceBySKIConnections(container, fromSKI, toSKI));
    }

    [Test]
    public void GetDistanceBySKIConnections_Unconnected()
    {
        SplineKnotIndex fromSKI = new SplineKnotIndex(2, 0);
        SplineKnotIndex toSKI = new SplineKnotIndex(3, 3);

        Assert.Throws<Exception>(() => Utils.GetDistanceBySKIConnections(container, fromSKI, toSKI));
    }

    [Test]
    public void ComputeCostStartNode_SameNode()
    {
        var startSplineKnotIndex = new SplineKnotIndex(0, 0);
        var endSplineKnotIndex = startSplineKnotIndex;

        AStarNode startNode = new AStarNode(container, startSplineKnotIndex);
        AStarNode endNode = new AStarNode(container, endSplineKnotIndex);

        startNode.ComputeCost(endNode, out float gCost, out float hCost);

        Assert.AreEqual(gCost, 0);
        Assert.AreEqual(hCost, 0);
    }

    [Test]
    public void ComputeCostStartNode_Neighbor()
    {
        // if you are changing this make sure that you make change so startNode and endNode are neighbors
        var startSplineKnotIndex = new SplineKnotIndex(0, 0);
        var endSplineKnotIndex = new SplineKnotIndex(0, 1);

        AStarNode startNode = new AStarNode(container, startSplineKnotIndex);
        AStarNode endNode = new AStarNode(container, endSplineKnotIndex);

        startNode.ComputeCost(endNode, out float gCost, out float hCost);

        Assert.AreEqual(gCost, 0);
        Assert.AreEqual(hCost, 50);
    }

    [Test]
    public void ComputeCostStartNode_UnconnectedDirectly() // We can't know if node is connected with the end node before we traverse
    {
        // if you change this, you will probably change the outcome of this test
        var startSplineKnotIndex = new SplineKnotIndex(0, 0);
        var endSplineKnotIndex = new SplineKnotIndex(3, 3);

        AStarNode startNode = new AStarNode(container, startSplineKnotIndex);
        AStarNode endNode = new AStarNode(container, endSplineKnotIndex);

        startNode.ComputeCost(endNode, out float gCost, out float hCost);

        BezierKnot startKnot, endKnot;
        startKnot = container[startNode.KnotLinksSet.First().Spline][startNode.KnotLinksSet.First().Knot];
        endKnot = container[endNode.KnotLinksSet.First().Spline][endNode.KnotLinksSet.First().Knot];
        Assert.AreEqual(0, gCost);
        Assert.AreEqual(Vector3.Distance(startKnot.Position, endKnot.Position), hCost);
    }

    [Test]
    public void ComputeCost_SameNode()
    {
        var parentSplineKnotIndex = new SplineKnotIndex(0, 0);
        var currentSplineKnotIndex = parentSplineKnotIndex;
        var endSplineKnotIndex = new SplineKnotIndex(3, 3);
        AStarNode.ParentConnection parentConnection = new AStarNode.ParentConnection(currentSplineKnotIndex, parentSplineKnotIndex);
        float parentNodeGCost = 0;
        float parentNodeHCost = math.sqrt(20000);

        AStarNode parentNode = new AStarNode(container, parentSplineKnotIndex)
        {
            gCost = parentNodeGCost,
            hCost = parentNodeHCost
        };
        AStarNode currentNode = new AStarNode(container, currentSplineKnotIndex);
        AStarNode endNode = new AStarNode(container, endSplineKnotIndex);

        var mockDistanceCalculator = new Mock<Utils.DistanceCalculatorDelegate>();
        mockDistanceCalculator
            .Setup(calculator => calculator(It.IsAny<SplineContainer>(), It.IsAny<SplineKnotIndex>(), It.IsAny<SplineKnotIndex>(), It.Is<Boolean>(x => x == false), It.Is<int>(x => x == 30)))
            .Returns((SplineContainer container, SplineKnotIndex fromSKI, SplineKnotIndex toSKI, bool approximate, int resolution) => { return 0; });

        currentNode.ComputeCost(parentNode, endNode, currentSplineKnotIndex, parentSplineKnotIndex, mockDistanceCalculator.Object, out float gCost, out float hCost);

        Assert.AreEqual(0, gCost);
        Assert.AreEqual(parentNodeHCost, hCost);
    }

    [Test]
    public void ComputeCost_Neighbor()
    {
        var parentSplineKnotIndex = new SplineKnotIndex(0, 0);
        var currentSplineKnotIndex = new SplineKnotIndex(0, 1);
        var endSplineKnotIndex = new SplineKnotIndex(3, 3);
        AStarNode.ParentConnection parentConnection = new AStarNode.ParentConnection(currentSplineKnotIndex, parentSplineKnotIndex);
        float parentNodeGCost = 0;
        float parentNodeHCost = math.sqrt(20000);
        float currentNodeHCost = math.sqrt(12500);

        AStarNode parentNode = new AStarNode(container, parentSplineKnotIndex)
        {
            gCost = parentNodeGCost,
            hCost = parentNodeHCost
        };
        AStarNode currentNode = new AStarNode(container, currentSplineKnotIndex);
        AStarNode endNode = new AStarNode(container, endSplineKnotIndex);

        var mockDistanceCalculator = new Mock<Utils.DistanceCalculatorDelegate>();
        var distanceToParent = mockDistanceCalculator
            .Setup(calculator => calculator(It.IsAny<SplineContainer>(), It.IsAny<SplineKnotIndex>(), It.IsAny<SplineKnotIndex>(), It.Is<Boolean>(x => x == false), It.Is<int>(x => x == 30)))
            .Returns((SplineContainer container, SplineKnotIndex fromSKI, SplineKnotIndex toSKI, bool approximate, int resolution) => { return 50f; });

        currentNode.ComputeCost(parentNode, endNode, parentSplineKnotIndex, currentSplineKnotIndex, mockDistanceCalculator.Object, out float gCost, out float hCost);

        Assert.AreEqual(parentNodeGCost + 50, gCost);
        Assert.AreEqual(currentNodeHCost, hCost);
    }

    [Test]
    public void ComputeCost_Unconnected()
    {
        // if you change this, you will probably change the outcome of this test
        var startSplineKnotIndex = new SplineKnotIndex(0, 0);
        var endSplineKnotIndex = new SplineKnotIndex(3, 3);

        AStarNode startNode = new AStarNode(container, startSplineKnotIndex);
        AStarNode endNode = new AStarNode(container, endSplineKnotIndex);

        startNode.ComputeCost(endNode, out float gCost, out float hCost);

        BezierKnot startKnot, endKnot;
        startKnot = container[startNode.KnotLinksSet.First().Spline][startNode.KnotLinksSet.First().Knot];
        endKnot = container[endNode.KnotLinksSet.First().Spline][endNode.KnotLinksSet.First().Knot];
        Assert.AreEqual(0, gCost);
        Assert.AreEqual(Vector3.Distance(startKnot.Position, endKnot.Position), hCost);
    }

    [Test]
    public void GetNeighbors_ManyNeighbors()
    {
        AStarNode currentNode = new AStarNode(container, new SplineKnotIndex(1, 1));
        HashSet<AStarNode> neighbors = currentNode.GetNeighbors();

        HashSet<AStarNode> expected = new HashSet<AStarNode>
        {
            new AStarNode(container, new SplineKnotIndex(0, 2)),
            new AStarNode(container, new SplineKnotIndex(0, 0)),
            new AStarNode(container, new SplineKnotIndex(1, 2)),
            new AStarNode(container, new SplineKnotIndex(1, 0)),
        };

        Assert.AreEqual(expected, neighbors);
        Assert.IsNotEmpty(neighbors);
    }

    [Test]
    public void GetNeighbors_NoNeighbors()
    {
        AStarNode currentNode = new AStarNode(container, new SplineKnotIndex(11117, 11117));
        HashSet<AStarNode> neighbors = currentNode.GetNeighbors();

        Assert.True(neighbors is HashSet<AStarNode>);
        Assert.IsEmpty(neighbors);
    }
}
