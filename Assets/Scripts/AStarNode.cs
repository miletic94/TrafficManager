using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

// public class KnotLinksSet
// {
//     private HashSet<SplineKnotIndex> k_KnotLinks;

//     public HashSet<SplineKnotIndex> KnotLinks => k_KnotLinks;

//     public KnotLinksSet(IReadOnlyList<SplineKnotIndex> knotLinks)
//     {
//         k_KnotLinks = SetKnotLinks(knotLinks);
//     }

//     private HashSet<SplineKnotIndex> SetKnotLinks(IReadOnlyList<SplineKnotIndex> knotLinks)
//     {
//         HashSet<SplineKnotIndex> set = new HashSet<SplineKnotIndex>();
//         foreach (SplineKnotIndex ski in knotLinks)
//         {
//             set.Add(ski);
//         }
//         return set;
//     }
//     public override int GetHashCode()
//     {
//         List<int> hashes = new List<int>();
//         int sumHash = 0;
//         foreach (SplineKnotIndex ski in k_KnotLinks)
//         {
//             hashes.Add(ski.GetHashCode());
//         }

//         // Multiply each ski hash with each ski hash
//         for (int i = 0; i <= hashes.Count - 1; i++)
//         {
//             for (int j = i + 1; j <= hashes.Count; j++)
//             {
//                 if (j == hashes.Count)
//                 {
//                     sumHash += hashes[i] * 1;
//                 }
//                 else
//                 {
//                     sumHash += hashes[i] * hashes[j];
//                 }
//             }
//         }
//         return sumHash;
//     }
//     public override bool Equals(object obj)
//     {
//         if (obj == null || GetType() != obj.GetType())
//             return false;

//         KnotLinksSet other = (KnotLinksSet)obj;

//         foreach (SplineKnotIndex ski in other.k_KnotLinks)
//         {
//             if (!k_KnotLinks.Contains(ski))
//             {
//                 return false;
//             }
//         }
//         return true;
//     }
// }

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


    public SortedSet<SplineKnotIndex> KnotLinksSet { get; }
    public AStarNode(IReadOnlyList<SplineKnotIndex> knotLinks)
    {
        KnotLinksSet = SetKnotLinks(knotLinks);
        gCost = float.PositiveInfinity;
        hCost = float.NegativeInfinity;
    }

    // public AStarNode(SplineContainer splineContainer, AStarNode start, AStarNode end, IReadOnlyList<SplineKnotIndex> knotLinks)
    // {
    //     SetKnotLinks(knotLinks);
    //     SetCost(splineContainer, start, end);
    // }

    // public AStarNode() { }

    private SortedSet<SplineKnotIndex> SetKnotLinks(IReadOnlyList<SplineKnotIndex> knotLinks)
    {
        SortedSet<SplineKnotIndex> set = new SortedSet<SplineKnotIndex>(new SplineKnotIndexComparer());
        foreach (SplineKnotIndex ski in knotLinks)
        {
            set.Add(ski);
        }
        return set;
    }

    //Probably redundant
    public void ComputeAndSetCost(SplineContainer splineContainer, AStarNode parentNode, AStarNode endNode)
    {
        float gCost, hCost;
        ComputeCost(splineContainer, parentNode, endNode, out gCost, out hCost);
        this.gCost = gCost;
        this.hCost = hCost;
    }

    public void ComputeAndSetCost(SplineContainer splineContainer, AStarNode endNode)
    {
        float gCost, hCost;
        ComputeCost(splineContainer, endNode, out gCost, out hCost);
        this.gCost = gCost;
        this.hCost = hCost;
    }

    // Compute fCost for node that has parent (is not starting node)
    public void ComputeCost(SplineContainer splineContainer, AStarNode parentNode, AStarNode endNode, out float gCost, out float hCost)
    {
        BezierKnot endKnot = splineContainer[endNode.KnotLinksSet.First().Spline][endNode.KnotLinksSet.First().Knot];
        BezierKnot currentKnot = splineContainer[KnotLinksSet.First().Spline][KnotLinksSet.First().Knot];

        // TODO: Better heuristics. For example gCost: distance between parent and current node + distance from parent to start node.  
        float distanceToParent;
        bool isDistanceToParent = TryGetDistanceToNode(splineContainer, parentNode, out distanceToParent);
        if (isDistanceToParent)
        {
            gCost = parentNode.gCost + distanceToParent;
            hCost = Vector3.Distance(currentKnot.Position, endKnot.Position);
        }
        else throw new InvalidOperationException("Trying to find distance between nodes that are not connected");
    }

    // Compute fCost for the start node
    public void ComputeCost(SplineContainer splineContainer, AStarNode endNode, out float gCost, out float hCost)
    {
        BezierKnot endKnot = splineContainer[endNode.KnotLinksSet.First().Spline][endNode.KnotLinksSet.First().Knot];
        BezierKnot currentKnot = splineContainer[KnotLinksSet.First().Spline][KnotLinksSet.First().Knot];
        gCost = 0;
        hCost = Vector3.Distance(currentKnot.Position, endKnot.Position);
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

    public bool TryFindKnotLinkToNode(SplineContainer splineContainer, AStarNode otherNode, out SplineKnotIndex fromSKI, out SplineKnotIndex toSKI)
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
            fromSKI = new SplineKnotIndex(-1, -1);
            toSKI = new SplineKnotIndex(-1, -1);
            return false;
        }
        fromSKI = KnotLinksSet.First();
        toSKI = KnotLinksSet.First();
        return true;
    }

    public bool TryGetDistanceToNode(SplineContainer splineContainer, AStarNode otherNode, out float distance, int resolution = 30)
    {
        if (!this.Equals(otherNode))
        {
            // fromConnectionSKI is not directly related to fromKnot because fromKnot has to be the one that comes first in the spline from the two.
            // Order matters in splines.
            // fromConnectionSKI refers to the SKI of the node currently under processing, while fromSKIOrdered refers to the SKI of the knot that precedes it in the spline sequence.
            SplineKnotIndex fromConnectionSKI, toConnectionSKI, fromSKIOrdered, toSKIOrdered;
            bool isLinkFound = TryFindKnotLinkToNode(splineContainer, otherNode, out fromConnectionSKI, out toConnectionSKI);
            Utils.OrderSKIsByKnot(fromConnectionSKI, toConnectionSKI, out fromSKIOrdered, out toSKIOrdered);

            if (isLinkFound)
            {
                BezierKnot fromKnot = splineContainer.Splines[fromSKIOrdered.Spline][fromSKIOrdered.Knot];
                BezierKnot toKnot = splineContainer.Splines[toSKIOrdered.Spline][toSKIOrdered.Knot];

                BezierCurve curve = new BezierCurve(fromKnot.Position, Utils.TangentWorldPosition(fromKnot, Utils.TangentType.TangentOut), Utils.TangentWorldPosition(toKnot, Utils.TangentType.TangentIn), toKnot.Position);

                distance = CurveUtility.CalculateLength(curve);

                Debug.Log($"Distance {distance}");

                return true;
            }
        }
        else
        {
            distance = 0;
            return true;
        }
        distance = -1;
        return false;
    }

    public bool TryGetApproximateDistanceToNode(SplineContainer splineContainer, AStarNode otherNode, out float distance)
    {
        if (!this.Equals(otherNode))
        {
            // fromConnectionSKI is not directly related to fromKnot because fromKnot has to be the one that comes first in the spline from the two.
            // Order matters in splines.
            // fromConnectionSKI refers to the SKI of the node currently under processing, while fromSKIOrdered refers to the SKI of the knot that precedes it in the spline sequence.
            SplineKnotIndex fromConnectionSKI, toConnectionSKI, fromSKIOrdered, toSKIOrdered;
            bool isLinkFound = TryFindKnotLinkToNode(splineContainer, otherNode, out fromConnectionSKI, out toConnectionSKI);
            Utils.OrderSKIsByKnot(fromConnectionSKI, toConnectionSKI, out fromSKIOrdered, out toSKIOrdered);
            if (isLinkFound)
            {
                BezierKnot fromKnot = splineContainer.Splines[fromSKIOrdered.Spline][fromSKIOrdered.Knot];
                BezierKnot toKnot = splineContainer.Splines[toSKIOrdered.Spline][toSKIOrdered.Knot];

                BezierCurve curve = new BezierCurve(fromKnot.Position, Utils.TangentWorldPosition(fromKnot, Utils.TangentType.TangentOut), Utils.TangentWorldPosition(toKnot, Utils.TangentType.TangentIn), toKnot.Position);
                distance = CurveUtility.ApproximateLength(curve);

                return true;
            }
        }
        else
        {
            distance = 0;
            return true;
        }
        distance = -1;
        return false;
    }

    public override string ToString()
    {
        String s = "";
        foreach (SplineKnotIndex ski in KnotLinksSet)
        {
            s += $"Spline: {ski.Spline}, Knot: {ski.Knot} ";
        }
        s += $"\n gCost: {gCost}, hCost: {hCost}, fCost: {gCost + hCost}";

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

// public class AStarNodeSKIEqualityComparer : IEqualityComparer<AStarNode>
// {
//     public int GetHashCode(AStarNode obj)
//     {
//         return obj.KnotLinksSet.GetHashCode();
//     }

//     public bool Equals(AStarNode x, AStarNode y)
//     {
//         return x.KnotLinksSet.Equals(y.KnotLinksSet);
//     }
// }

// public class AStarNodeComparer : IComparer<AStarNode>
// {
//     public int Compare(AStarNode x, AStarNode y)
//     {
//         return x.fCost.CompareTo(y.fCost);
//     }
// }

// class StartAStarNode : AStarNode
// {
//     private static StartAStarNode instance;
//     private static readonly object lockedObject = new object();
//     private StartAStarNode(SplineContainer splineContainer, AStarNode endNode, IReadOnlyList<SplineKnotIndex> knotLinks)
//     {
//         k_KnotLinks = knotLinks;
//         SetCost(splineContainer, this, endNode);
//     }
//     public static StartAStarNode Instance => instance;

//     public static StartAStarNode GetInstance(SplineContainer splineContainer, AStarNode endNode, IReadOnlyList<SplineKnotIndex> knotLinks)
//     {
//         lock (lockedObject)
//         {
//             if (instance == null)
//             {
//                 instance = new StartAStarNode(splineContainer, endNode, knotLinks);
//             }
//             return instance;
//         }
//     }

//     public static void RemoveInstance()
//     {
//         instance = null;
//     }
// }

// class EndAStarNode : AStarNode
// {
//     private static EndAStarNode instance;
//     private static readonly object lockedObject = new object();
//     private EndAStarNode(SplineContainer splineContainer, AStarNode endNode, IReadOnlyList<SplineKnotIndex> knotLinks)
//     {
//         k_KnotLinks = knotLinks;
//         SetCost(splineContainer, this, endNode);
//     }
//     public EndAStarNode Instance => instance;

//     public static EndAStarNode GetInstance(SplineContainer splineContainer, AStarNode endNode, IReadOnlyList<SplineKnotIndex> knotLinks)
//     {
//         lock (lockedObject)
//         {
//             if (StartAStarNode.Instance != null)
//             {
//                 if (instance == null)
//                 {
//                     instance = new EndAStarNode(splineContainer, endNode, knotLinks);
//                 }
//                 return instance;
//             }
//             else
//             {
//                 throw new InvalidOperationException("StartAStarNode should exist before creating EndAStarNode instance");
//             }
//         }
//     }

//     public static void RemoveInstance()
//     {
//         instance = null;
//     }
// }