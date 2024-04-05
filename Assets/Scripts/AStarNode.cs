using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class AStarNode : IEquatable<AStarNode>, IComparable<AStarNode>
{
    public float fCost { get => gCost + hCost; }
    public float gCost { get; set; }
    public float hCost { get; set; }
    public AStarNode parent;
    public SplineContainer SplineContainer;

    public ParentConnection parentConnection;

    public SortedSet<SplineKnotIndex> KnotLinksSet { get; }

    public class ParentConnection
    {
        public SplineKnotIndex CurrentSKI;

        public SplineKnotIndex ParentSKI;

        public ParentConnection()
        {
            CurrentSKI = SplineKnotIndex.Invalid;
            ParentSKI = SplineKnotIndex.Invalid;
        }

        public ParentConnection(SplineKnotIndex currentSKI, SplineKnotIndex parentSKI)
        {
            CurrentSKI = currentSKI;
            ParentSKI = parentSKI;
        }
    }
    public AStarNode(SplineContainer splineContainer, SplineKnotIndex splineKnotIndex)
    {
        SplineContainer = splineContainer;
        KnotLinksSet = SetKnotLinks(splineContainer.KnotLinkCollection.GetKnotLinks(splineKnotIndex));
        gCost = float.PositiveInfinity;
        hCost = float.NegativeInfinity;
        parentConnection = new ParentConnection();
    }

    private SortedSet<SplineKnotIndex> SetKnotLinks(IReadOnlyList<SplineKnotIndex> knotLinks)
    {
        if (knotLinks.Count < 1) throw new Exception("KnotLinksSet is empty");
        SortedSet<SplineKnotIndex> set = new SortedSet<SplineKnotIndex>(new Comparers.SplineKnotIndexComparer.SplineKnotIndexComparer());
        foreach (SplineKnotIndex ski in knotLinks)
        {
            set.Add(ski);
        }
        return set;
    }

    public void SetParentConnection(ParentConnection parentConnection)
    {
        this.parentConnection.CurrentSKI = parentConnection.CurrentSKI;
        this.parentConnection.ParentSKI = parentConnection.ParentSKI;
    }

    public void SetStartNodeCost(AStarNode endNode)
    {
        float gCost, hCost;
        ComputeCost(endNode, out gCost, out hCost);
        this.gCost = gCost;
        this.hCost = hCost;
    }

    // Compute fCost for node that has parent (is not starting node)
    public void ComputeCost(AStarNode parentNode, AStarNode endNode, ParentConnection parentConnection, out float gCost, out float hCost)
    {
        float distanceToParent = GetDistanceBySKIConnections(parentConnection.CurrentSKI, parentConnection.ParentSKI);

        gCost = parentNode.gCost + distanceToParent;
        hCost = GetHCost(endNode);
    }

    // Compute fCost for the start node
    public void ComputeCost(AStarNode endNode, out float gCost, out float hCost)
    {
        gCost = 0;
        hCost = GetHCost(endNode);
    }

    float GetHCost(AStarNode endNode)
    {
        BezierKnot endKnot = SplineContainer[endNode.KnotLinksSet.First().Spline][endNode.KnotLinksSet.First().Knot];
        BezierKnot currentKnot = SplineContainer[KnotLinksSet.First().Spline][KnotLinksSet.First().Knot];

        return Vector3.Distance(currentKnot.Position, endKnot.Position);
    }

    public HashSet<AStarNode> GetNeighbors()
    {
        HashSet<AStarNode> neighbors = new HashSet<AStarNode>();
        foreach (SplineKnotIndex splineKnotIndex in KnotLinksSet)
        {
            Spline spline = SplineContainer[splineKnotIndex.Spline];
            int nextKnotIndex = splineKnotIndex.Knot + 1;
            int prevKnotIndex = splineKnotIndex.Knot - 1;
            if (nextKnotIndex < spline.Count)
            {
                SplineKnotIndex nextSplineKnotIndex = new SplineKnotIndex(splineKnotIndex.Spline, nextKnotIndex);
                neighbors.Add(new AStarNode(SplineContainer, nextSplineKnotIndex));
            }

            if (prevKnotIndex > -1)
            {
                SplineKnotIndex prevSplineKnotIndex = new SplineKnotIndex(splineKnotIndex.Spline, prevKnotIndex);
                neighbors.Add(new AStarNode(SplineContainer, prevSplineKnotIndex));
            }
        }
        return neighbors;
    }

    public bool TryFindSKIConnectionToNode(AStarNode otherNode, out SplineKnotIndex currentSKI, out SplineKnotIndex otherSKI)
    {
        if (!this.Equals(otherNode))
        {
            foreach (SplineKnotIndex processingSKICurrent in KnotLinksSet)
            {
                Spline spline = SplineContainer[processingSKICurrent.Spline];
                int nextKnot = processingSKICurrent.Knot + 1;
                int prevKnot = processingSKICurrent.Knot - 1;

                if (prevKnot >= 0)
                {
                    SplineKnotIndex processingSKIOther;
                    bool isFound = otherNode.KnotLinksSet.TryGetValue(new SplineKnotIndex(processingSKICurrent.Spline, prevKnot), out processingSKIOther);

                    if (isFound)
                    {
                        currentSKI = processingSKICurrent;
                        otherSKI = processingSKIOther;
                        return true;
                    }
                }
                if (nextKnot < spline.Count)
                {
                    SplineKnotIndex processingSKIOther;
                    bool isFound = otherNode.KnotLinksSet.TryGetValue(new SplineKnotIndex(processingSKICurrent.Spline, nextKnot), out processingSKIOther);

                    if (isFound)
                    {
                        currentSKI = processingSKICurrent;
                        otherSKI = processingSKIOther;
                        return true;
                    }
                }
            }
            currentSKI = SplineKnotIndex.Invalid;
            otherSKI = SplineKnotIndex.Invalid;
            return false;
        }
        SplineKnotIndex anySKI = KnotLinksSet.First(); // If nodes are the same any knot will work and it will connect to itself
        currentSKI = anySKI;
        otherSKI = anySKI;
        return true;
    }

    // Use approximate to switch algorithm by which curve length is calculated.
    // approximate = false will use CurveUtility.CalculateLength which is more precise, but slower. Here resolution parameter is used as precision measure.
    // approximate = true will use CurveUtility.ApproximateLength which is faster, but less precise.
    public float GetDistanceBySKIConnections(SplineKnotIndex fromSKI, SplineKnotIndex toSKI, bool approximate = false, int resolution = 30)
    {
        if (fromSKI.Equals(toSKI))
        {
            return 0;
        }

        SplineKnotIndex fromSKIOrdered, toSKIOrdered;
        Utils.OrderSKIsByKnot(fromSKI, toSKI, out fromSKIOrdered, out toSKIOrdered);

        BezierKnot fromKnot = SplineContainer.Splines[fromSKIOrdered.Spline][fromSKIOrdered.Knot];
        BezierKnot toKnot = SplineContainer.Splines[toSKIOrdered.Spline][toSKIOrdered.Knot];
        BezierCurve curve = new BezierCurve(fromKnot.Position, toKnot.Position);

        if (approximate)
        {
            return CurveUtility.ApproximateLength(curve);
        }
        return CurveUtility.CalculateLength(curve, resolution);
    }

    public override string ToString()
    {
        String s = "";
        foreach (SplineKnotIndex ski in KnotLinksSet)
        {
            s += $"Spline: {ski.Spline}, Knot: {ski.Knot} ";
        }
        s += $"\n gCost: {gCost}, hCost: {hCost}, fCost: {gCost + hCost} currentSKI {parentConnection.CurrentSKI} parentSKI {parentConnection.ParentSKI}";

        return s;
    }

    public override int GetHashCode()
    {
        var comparer = new Comparers.KnotLinkSetComparer.KnotLinkSetEqualityComparer();

        return comparer.GetHashCode(KnotLinksSet);
    }

    public bool Equals(AStarNode other)
    {
        if (other.GetType() == null || GetType() != other.GetType())
        {
            return false;
        }
        var comparer = new Comparers.KnotLinkSetComparer.KnotLinkSetEqualityComparer();
        return comparer.Equals(KnotLinksSet, other.KnotLinksSet);
    }

    public int CompareTo(AStarNode other)
    {
        int fCostComparison = fCost.CompareTo(other.fCost);
        if (fCostComparison != 0)
        {
            return fCostComparison;
        }

        var SKIComparer = new Comparers.SplineKnotIndexComparer.SplineKnotIndexComparer();
        return SKIComparer.Compare(KnotLinksSet.First(), other.KnotLinksSet.First()); // Since KnotLinkSet is SortedSet, if KnotLinkSet.First() is equal, then KnotLinkSets will be equal
    }
}