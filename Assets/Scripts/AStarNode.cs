using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class KnotLinksSet
{
    private HashSet<SplineKnotIndex> k_KnotLinks;

    public HashSet<SplineKnotIndex> KnotLinks => k_KnotLinks;

    public KnotLinksSet(IReadOnlyList<SplineKnotIndex> knotLinks)
    {
        k_KnotLinks = SetKnotLinks(knotLinks);
    }

    private HashSet<SplineKnotIndex> SetKnotLinks(IReadOnlyList<SplineKnotIndex> knotLinks)
    {
        HashSet<SplineKnotIndex> set = new HashSet<SplineKnotIndex>();
        foreach (SplineKnotIndex ski in knotLinks)
        {
            set.Add(ski);
        }
        return set;
    }
    public override int GetHashCode()
    {
        List<int> hashes = new List<int>();
        int sumHash = 0;
        foreach (SplineKnotIndex ski in k_KnotLinks)
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
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        KnotLinksSet other = (KnotLinksSet)obj;

        foreach (SplineKnotIndex ski in other.k_KnotLinks)
        {
            if (!k_KnotLinks.Contains(ski))
            {
                return false;
            }
        }
        return true;
    }
}
public class AStarNode : IEquatable<AStarNode>, IComparable<AStarNode>
{
    public float fCost { get => gCost + hCost; }
    public float gCost { get; set; }
    public float hCost { get; set; }

    KnotLinksSet k_KnotLinksSet;
    public KnotLinksSet KnotLinksSet => k_KnotLinksSet;
    public AStarNode(IReadOnlyList<SplineKnotIndex> knotLinks)
    {
        k_KnotLinksSet = new KnotLinksSet(knotLinks);
    }

    public AStarNode(SplineContainer splineContainer, AStarNode start, AStarNode end, IReadOnlyList<SplineKnotIndex> knotLinks)
    {
        k_KnotLinksSet = new KnotLinksSet(knotLinks);
        SetCost(splineContainer, start, end);
    }

    public AStarNode() { }


    //TODO: Should have ComputeCost and SetCost, so I could compare possible cost with existing cost before setting it;
    public void SetCost(SplineContainer splineContainer, AStarNode start, AStarNode end)
    {
        BezierKnot startKnot = splineContainer[start.k_KnotLinksSet.KnotLinks.First().Spline][start.k_KnotLinksSet.KnotLinks.First().Knot];
        BezierKnot endKnot = splineContainer[end.k_KnotLinksSet.KnotLinks.First().Spline][end.k_KnotLinksSet.KnotLinks.First().Knot];
        BezierKnot currentKnot = splineContainer[k_KnotLinksSet.KnotLinks.First().Spline][k_KnotLinksSet.KnotLinks.First().Knot];

        // TODO: Better heuristics. For example gCost: distance between parent and current node + distance from parent to start node.  
        gCost = Vector3.Distance(currentKnot.Position, startKnot.Position);
        hCost = Vector3.Distance(currentKnot.Position, endKnot.Position);
    }

    public HashSet<AStarNode> GetNeighbors(SplineContainer splineContainer)
    {
        HashSet<AStarNode> neighbors = new HashSet<AStarNode>();
        foreach (SplineKnotIndex splineKnotIndex in k_KnotLinksSet.KnotLinks)
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

    public override string ToString()
    {
        String s = "";
        foreach (SplineKnotIndex ski in k_KnotLinksSet.KnotLinks)
        {
            s += $"Spline: {ski.Spline}, Knot: {ski.Knot} ";
        }
        s += $"\n gCost: {gCost}, hCost: {hCost}, fCost: {gCost + hCost}";

        return s;
    }

    public override int GetHashCode()
    {
        return KnotLinksSet.GetHashCode();
    }

    public bool Equals(AStarNode other)
    {
        if (other.GetType() == null || GetType() != other.GetType())
        {
            return false;
        }

        return KnotLinksSet.Equals(other.KnotLinksSet);
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