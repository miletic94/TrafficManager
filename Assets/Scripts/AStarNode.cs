using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class KnotLinksEqualityComparer : IEqualityComparer<SortedSet<SplineKnotIndex>>
{
    public int GetHashCode(SortedSet<SplineKnotIndex> x)
    {
        List<int> hashes = new List<int>();
        int sumHash = 0;
        foreach (SplineKnotIndex ski in x)
        {
            hashes.Add(ski.GetHashCode());
        }

        // Multiply each ski hash with each ski hash
        for (int i = 0; i <= hashes.Count - 1; i++)
        {
            for (int j = i + 1; j <= hashes.Count; j++)
            {
                if (j == hashes.Count)
                {
                    sumHash += hashes[i] * 1;
                }
                else
                {
                    sumHash += hashes[i] * hashes[j];
                }
            }
        }
        return sumHash;
    }
    public bool Equals(SortedSet<SplineKnotIndex> x, SortedSet<SplineKnotIndex> y)
    {

        foreach (SplineKnotIndex ski in x)
        {
            if (!y.Contains(ski))
            {
                return false;
            }
        }
        return true;
    }
}

public class SplineKnotIndexComparer : IComparer<SplineKnotIndex>
{
    public int Compare(SplineKnotIndex x, SplineKnotIndex y)
    {
        if (x.Spline < y.Spline)
            return -1;
        else if (x.Spline > y.Spline)
            return 1;
        else
        {
            // If Spline values are equal, compare Knot values
            if (x.Knot < y.Knot)
                return -1;
            else if (x.Knot > y.Knot)
                return 1;
            else
                return 0; // SKIs are equal
        }
    }
}
public class AStarNode : IEquatable<AStarNode>, IComparable<AStarNode>
{
    public float fCost { get => gCost + hCost; }
    public float gCost { get; set; }
    public float hCost { get; set; }
    public AStarNode parent;

    // TODO: Maybe make this class
    public (SplineKnotIndex currentSKI, SplineKnotIndex parentSKI) parentCurrentSKIConnection;

    public SortedSet<SplineKnotIndex> KnotLinksSet { get; }
    public AStarNode(IReadOnlyList<SplineKnotIndex> knotLinks)
    {
        KnotLinksSet = SetKnotLinks(knotLinks);
        gCost = float.PositiveInfinity;
        hCost = float.NegativeInfinity;
    }

    private SortedSet<SplineKnotIndex> SetKnotLinks(IReadOnlyList<SplineKnotIndex> knotLinks)
    {
        SortedSet<SplineKnotIndex> set = new SortedSet<SplineKnotIndex>(new SplineKnotIndexComparer());
        foreach (SplineKnotIndex ski in knotLinks)
        {
            set.Add(ski);
        }
        return set;
    }

    public void SetStartNodeCost(SplineContainer splineContainer, AStarNode endNode)
    {
        float gCost, hCost;
        ComputeCost(splineContainer, endNode, out gCost, out hCost);
        this.gCost = gCost;
        this.hCost = hCost;
    }

    // Compute fCost for node that has parent (is not starting node)
    public void ComputeCost(SplineContainer splineContainer, AStarNode parentNode, AStarNode endNode, (SplineKnotIndex fromSKI, SplineKnotIndex toSKI) parentCurrentNodeSKIConnection, out float gCost, out float hCost)
    {
        float distanceToParent = GetDistanceBySKIConnections(splineContainer, parentCurrentNodeSKIConnection.fromSKI, parentCurrentNodeSKIConnection.toSKI);

        gCost = parentNode.gCost + distanceToParent;
        hCost = GetHCost(splineContainer, endNode);
    }

    // Compute fCost for the start node
    public void ComputeCost(SplineContainer splineContainer, AStarNode endNode, out float gCost, out float hCost)
    {
        gCost = 0;
        hCost = GetHCost(splineContainer, endNode);
    }

    float GetHCost(SplineContainer splineContainer, AStarNode endNode)
    {
        BezierKnot endKnot = splineContainer[endNode.KnotLinksSet.First().Spline][endNode.KnotLinksSet.First().Knot];
        BezierKnot currentKnot = splineContainer[KnotLinksSet.First().Spline][KnotLinksSet.First().Knot];

        return Vector3.Distance(currentKnot.Position, endKnot.Position);
    }

    public HashSet<AStarNode> GetNeighbors(SplineContainer splineContainer)
    {
        HashSet<AStarNode> neighbors = new HashSet<AStarNode>();
        foreach (SplineKnotIndex splineKnotIndex in KnotLinksSet)
        {
            Spline spline = splineContainer[splineKnotIndex.Spline];
            int nextKnotIndex = splineKnotIndex.Knot + 1;
            int prevKnotIndex = splineKnotIndex.Knot - 1;
            if (nextKnotIndex < spline.Count)
            {
                SplineKnotIndex nextSplineKnotIndex = new SplineKnotIndex(splineKnotIndex.Spline, nextKnotIndex);
                neighbors.Add(new AStarNode(splineContainer.KnotLinkCollection.GetKnotLinks(nextSplineKnotIndex)));
            }

            if (prevKnotIndex > -1)
            {
                SplineKnotIndex prevSplineKnotIndex = new SplineKnotIndex(splineKnotIndex.Spline, prevKnotIndex);
                neighbors.Add(new AStarNode(splineContainer.KnotLinkCollection.GetKnotLinks(prevSplineKnotIndex)));
            }
        }
        return neighbors;
    }

    public bool TryFindSKILinkToNode(SplineContainer splineContainer, AStarNode otherNode, out SplineKnotIndex fromSKI, out SplineKnotIndex toSKI)
    {
        if (!this.Equals(otherNode))
        {
            foreach (SplineKnotIndex ski in KnotLinksSet)
            {
                Spline spline = splineContainer[ski.Spline];
                int nextKnot = ski.Knot + 1;
                int prevKnot = ski.Knot - 1;

                if (prevKnot >= 0)
                {
                    SplineKnotIndex SKI;
                    bool isFound = otherNode.KnotLinksSet.TryGetValue(new SplineKnotIndex(ski.Spline, prevKnot), out SKI);

                    if (isFound)
                    {
                        fromSKI = ski;
                        toSKI = SKI;
                        return true;
                    }
                }
                if (nextKnot < spline.Count)
                {
                    SplineKnotIndex SKI;
                    bool isFound = otherNode.KnotLinksSet.TryGetValue(new SplineKnotIndex(ski.Spline, nextKnot), out SKI);

                    if (isFound)
                    {
                        fromSKI = ski;
                        toSKI = SKI;
                        return true;
                    }
                }
            }
            fromSKI = SplineKnotIndex.Invalid;
            toSKI = SplineKnotIndex.Invalid;
            return false;
        }
        // TODO: .First() is not safe. It could fail if Set is empty. It is also magic value.
        fromSKI = KnotLinksSet.First();
        toSKI = KnotLinksSet.First();
        return true;
    }

    // Use approximate to switch algorithm by which curve length is calculated.
    // approximate = false will use CurveUtility.CalculateLength which is more precise, but slower. Here resolution parameter is used as precision measure.
    // approximate = true will use CurveUtility.ApproximateLength which is faster, but less precise.
    public float GetDistanceBySKIConnections(SplineContainer splineContainer, SplineKnotIndex fromSKI, SplineKnotIndex toSKI, bool approximate = false, int resolution = 30)
    {
        if (fromSKI.Equals(toSKI))
        {
            return 0;
        }

        SplineKnotIndex fromSKIOrdered, toSKIOrdered;
        Utils.OrderSKIsByKnot(fromSKI, toSKI, out fromSKIOrdered, out toSKIOrdered);

        BezierKnot fromKnot = splineContainer.Splines[fromSKIOrdered.Spline][fromSKIOrdered.Knot];
        BezierKnot toKnot = splineContainer.Splines[toSKIOrdered.Spline][toSKIOrdered.Knot];

        BezierCurve curve = new BezierCurve(fromKnot.Position, Utils.TangentWorldPosition(fromKnot, Utils.TangentType.TangentOut), Utils.TangentWorldPosition(toKnot, Utils.TangentType.TangentIn), toKnot.Position);

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
        s += $"\n gCost: {gCost}, hCost: {hCost}, fCost: {gCost + hCost} currentSKI {parentCurrentSKIConnection.currentSKI} parentSKI {parentCurrentSKIConnection.parentSKI}";

        return s;
    }

    public override int GetHashCode()
    {
        var comparer = new KnotLinksEqualityComparer();

        return comparer.GetHashCode(KnotLinksSet);
    }

    public bool Equals(AStarNode other)
    {
        if (other.GetType() == null || GetType() != other.GetType())
        {
            return false;
        }
        var comparer = new KnotLinksEqualityComparer();
        return comparer.Equals(KnotLinksSet, other.KnotLinksSet);
    }

    public int CompareTo(AStarNode other)
    {
        return fCost.CompareTo(other.fCost);
    }
}