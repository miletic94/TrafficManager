using System;
using System.Collections.Generic;
using System.Linq;
using Codice.ThemeImages;
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
        KnotLinksSet = SetKnotLinks(splineKnotIndex);
        gCost = float.PositiveInfinity;
        hCost = float.NegativeInfinity;
        parentConnection = new ParentConnection();
    }

    private SortedSet<SplineKnotIndex> SetKnotLinks(SplineKnotIndex splineKnotIndex)
    {
        IReadOnlyList<SplineKnotIndex> knotLinks = SplineContainer.KnotLinkCollection.GetKnotLinks(splineKnotIndex);
        if (knotLinks.Count == 0) throw new Exception("KnotLinksSet is empty"); // This will probably never happen due to how GetKnotLinks works;
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
    // Compute cost for node that has parent (is not starting node)
    public void ComputeCost(AStarNode parentNode, AStarNode endNode, SplineKnotIndex fromSKI, SplineKnotIndex toSKI, Utils.DistanceCalculatorDelegate distanceCalculator, out float gCost, out float hCost)
    {
        if (parentNode.fCost == float.NaN) throw new ArgumentException("fCost shouldn't be NaN");

        float distanceToParent = distanceCalculator(SplineContainer, fromSKI, toSKI);

        gCost = parentNode.gCost + distanceToParent;
        hCost = ComputeHCost(endNode);
    }

    // Compute cost for the start node
    public void ComputeCost(AStarNode endNode, out float gCost, out float hCost)
    {
        gCost = 0;
        hCost = ComputeHCost(endNode);
    }

    float ComputeHCost(AStarNode endNode)
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
            int nextKnotIndex = splineKnotIndex.Knot + 1;
            int prevKnotIndex = splineKnotIndex.Knot - 1;
            SplineKnotIndex nextSplineKnotIndex = new SplineKnotIndex(splineKnotIndex.Spline, nextKnotIndex);
            SplineKnotIndex prevSplineKnotIndex = new SplineKnotIndex(splineKnotIndex.Spline, prevKnotIndex);
            if (IsIndexValid(nextSplineKnotIndex))
            {
                neighbors.Add(new AStarNode(SplineContainer, nextSplineKnotIndex));
            }

            if (IsIndexValid(prevSplineKnotIndex))
            {
                neighbors.Add(new AStarNode(SplineContainer, prevSplineKnotIndex));
            }

        }
        return neighbors;
    }

    internal bool IsIndexValid(SplineKnotIndex index)
    {
        return index.Spline < SplineContainer.Splines.Count && index.Knot >= 0 && index.Knot < SplineContainer.Splines[index.Spline].Count
             && index.Knot < SplineContainer.Splines[index.Spline].Count;
    }

    /// <summary>
    /// Attempts to find a connection between the current AStarNode and the specified AStarNode.
    /// If a connection is found via a KnotLink connection, assigns the SplineKnotIndex
    /// of the current node and the other node to the out variables.
    /// </summary>
    /// <param name="otherNode">The other AStarNode to check for connection.</param>
    /// <param name="currentSKI">Output parameter: The SplineKnotIndex of the current node if connected.</param>
    /// <param name="otherSKI">Output parameter: The SplineKnotIndex of the other node if connected.</param>
    /// <returns>
    /// Returns true if a connection is found between the current node and the other node,
    /// and assigns the respective SplineKnotIndex values to the out parameters.
    /// Returns false if no connection is found, in which case the out parameters are set to <see cref="SplineKnotIndex.Invalid"/.
    /// </returns>
    /// <remarks>
    /// This method iterates through the KnotLinksSet of the current node and checks for connections 
    /// with the KnotLinksSet of the other node. If a connection is found, it assigns the SplineKnotIndex 
    /// values to the out parameters and returns true. If no connection is found, it sets the out parameters 
    /// to SplineKnotIndex.Invalid and returns false.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if otherNode is null.</exception>

    public bool TryFindSKIConnectionToNode(AStarNode otherNode, out SplineKnotIndex currentSKI, out SplineKnotIndex otherSKI)
    {
        if (otherNode == null) throw new ArgumentNullException("Other node can't be null");
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

    /// <summary>
    /// Attempts to find a connection between the current node and the specified node.
    /// If a connection is found via a KnotLink connection, assigns the SplineKnotIndex
    /// of the current node and the other node to the out variables.
    /// Uses the TryFindSKIConnectionToNode method internally.
    /// </summary>
    /// <param name="otherNode">The other node to check for connection.</param>
    /// <param name="currentSKI">The SplineKnotIndex of the current node if connected.</param>
    /// <param name="otherSKI">The SplineKnotIndex of the other node if connected.</param>
    /// <exception cref="Exception">
    /// Thrown if nodes do not have the necessary spline knot indexes 
    /// by which they can be connected.
    /// </exception>

    public void FindSKIConnectionToNode(AStarNode otherNode, out SplineKnotIndex currentSKI, out SplineKnotIndex otherSKI)
    {

        bool isConnection = TryFindSKIConnectionToNode(otherNode, out currentSKI, out otherSKI);

        if (!isConnection) throw new Exception("Potential neighbor is not linked to parent");
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