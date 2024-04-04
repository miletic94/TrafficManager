using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class Travel : MonoBehaviour
{
    [SerializeField]
    SplineContainer roadSplineContainer;
    [SerializeField]
    private SplineContainer splineContainer;
    private SplineAnimate splineAnimate;

    void Awake()
    {
        Debug.ClearDeveloperConsole();
        splineAnimate = GetComponent<SplineAnimate>();

        AStarNode startNode = new AStarNode(roadSplineContainer.KnotLinkCollection.GetKnotLinks(new SplineKnotIndex(1, 0)));
        AStarNode endNode = new AStarNode(roadSplineContainer.KnotLinkCollection.GetKnotLinks(new SplineKnotIndex(2, 0)));

        LinkedList<AStarNode> path = ShortestPath(roadSplineContainer, startNode, endNode);
        SplinePath splinePath = GenerateSplinePath(roadSplineContainer, GenerateSplineSliceInfo(path));
        Spline newSpline = GenerateSpline(splinePath);

        splineContainer.Splines = new Spline[1] { newSpline };
        splineAnimate.Container = splineContainer;
        splineAnimate.Loop = SplineAnimate.LoopMode.PingPong;
    }

    public enum TangentType
    {
        TangentIn,
        TangentOut
    }

    LinkedList<AStarNode> ShortestPath(SplineContainer splineContainer, AStarNode startNode, AStarNode endNode)
    {

        startNode.SetStartNodeCost(splineContainer, endNode);

        HashSet<AStarNode> visited = new HashSet<AStarNode>();
        //TODO: Implement heap
        SortedSet<AStarNode> heap = new SortedSet<AStarNode>()
        {
            startNode
        };
        while (heap.Count > 0)
        {
            AStarNode current = heap.First();
            heap.Remove(current);
            visited.Add(current);

            if (current.Equals(endNode))
            {
                LinkedList<AStarNode> path = GeneratePath(current);
                return path;
            }

            HashSet<AStarNode> neighbors = current.GetNeighbors(splineContainer);
            foreach (AStarNode n in neighbors)
            {

                if (!visited.Contains(n))
                {
                    AStarNode heapN;
                    bool heapContainsN = heap.TryGetValue(n, out heapN);

                    SplineKnotIndex currentSKI, parentSKI;
                    bool isLinkedToCurrent = n.TryFindSKIConnectionToNode(splineContainer, current, out currentSKI, out parentSKI);
                    if (!isLinkedToCurrent) throw new Exception("Potential neighbor is not linked to parent");

                    AStarNode.ParentConnection parentConnection = new AStarNode.ParentConnection(currentSKI, parentSKI);
                    float fCost, gCost, hCost;

                    n.ComputeCost(splineContainer, current, endNode, parentConnection, out gCost, out hCost);
                    fCost = gCost + hCost;

                    if (heapContainsN)
                    {
                        if (heapN.fCost > fCost)
                        {
                            heapN.gCost = gCost;
                            heapN.hCost = hCost;
                            heapN.parent = current;
                            heapN.SetParentConnection(parentConnection);
                        }
                    }
                    else
                    {
                        n.gCost = gCost;
                        n.hCost = hCost;
                        n.parent = current;
                        n.SetParentConnection(parentConnection);
                        heap.Add(n);
                    }
                }
            }
        }

        throw new Exception($"There is no path between node with SKI {startNode} and {endNode}");
    }

    // TODO: Create Path class or interface in order to use it here and in GenerateSplinePath as a type.
    LinkedList<AStarNode> GeneratePath(AStarNode endNode)
    {
        LinkedList<AStarNode> path = new LinkedList<AStarNode>();
        AStarNode current = endNode;

        path.AddFirst(current);

        while (current.parent != null)
        {
            current = current.parent;
            path.AddFirst(current);
        }
        return path;
    }


    // TODO: Use Path here as a type.
    List<SplineSliceInfo> GenerateSplineSliceInfo(LinkedList<AStarNode> path)
    {
        List<SplineSliceInfo> splineSliceInfoList = new List<SplineSliceInfo>();
        int previousSplineIndex = -1;
        SliceDirection previousDirection = SliceDirection.Forward;
        while (path.Count > 0)
        {
            AStarNode currentNode = path.FirstOrDefault(); // since count > 0 this should always have value;
            SplineKnotIndex currentSKI = currentNode.parentConnection.CurrentSKI;
            SplineKnotIndex parentSKI = currentNode.parentConnection.ParentSKI;

            SliceDirection currentDirection = parentSKI.Knot < currentSKI.Knot ? SliceDirection.Forward : SliceDirection.Backward;
            if (currentSKI.IsValid() && parentSKI.IsValid())
            {
                int currentSplineIndex = parentSKI.Spline; // currentSKI.SPline would be the same
                // TODO: Test change of the direction when that becomes possible
                if (currentSplineIndex != previousSplineIndex || currentDirection != previousDirection)
                {
                    previousSplineIndex = currentSplineIndex;
                    previousDirection = currentDirection;
                    SplineSliceInfo currentSplineSliceInfo = new SplineSliceInfo(currentSplineIndex, parentSKI.Knot, 2, currentDirection); // Here only parentSKI.Knot would be valid
                    splineSliceInfoList.Add(currentSplineSliceInfo);
                }
                else
                {
                    int lastSliceIndex = splineSliceInfoList.Count - 1;
                    SplineSliceInfo lastSliceInfo = splineSliceInfoList[lastSliceIndex];
                    lastSliceInfo.KnotsCount += 1;
                }
            }

            path.RemoveFirst();
        }
        return splineSliceInfoList;
    }

    SplinePath GenerateSplinePath(SplineContainer splineContainer, List<SplineSliceInfo> splineSliceInfos)
    {
        List<SplineSlice<Spline>> splinePathSlices = new List<SplineSlice<Spline>>();

        foreach (SplineSliceInfo ssi in splineSliceInfos)
        {
            splinePathSlices.Add(new SplineSlice<Spline>(splineContainer.Splines[ssi.SplineIndex], new SplineRange(ssi.StartingKnot, ssi.KnotsCount, ssi.Direction)));
        }

        return new SplinePath(splinePathSlices);
    }

    Spline GenerateSpline(SplinePath splinePath, bool closed = false)
    {
        List<BezierKnot> splineKnots = new List<BezierKnot>();

        for (int i = 0; i < splinePath.Count; i++)
        {

            // Knots belong to the same node (They are of the same position)
            if ((Vector3)splinePath[i].Position == (Vector3)splinePath[i + 1].Position)
            {
                BezierKnot firstKnot = splinePath[i];
                BezierKnot secondKnot = splinePath[i + 1];
                BezierKnot newKnot = new BezierKnot(firstKnot.Position, firstKnot.TangentIn, Utils.RotateVector(firstKnot.Rotation, secondKnot.Rotation, secondKnot.TangentOut), firstKnot.Rotation);
                splineKnots.Add(newKnot);

                i++; // SKip next knot since this and next knot will belong to the same node. If knots belong to the same node, that means that we need to use 
            }
            else
            {
                splineKnots.Add(splinePath[i]);
            }
        }
        return new Spline(splineKnots, closed);
    }

    class SplineSliceInfo
    {
        public int SplineIndex;
        public int StartingKnot;
        public int KnotsCount;
        public SliceDirection Direction;


        public SplineSliceInfo(int splineIndex, int startingKnot, int knotsCount, SliceDirection direction)
        {
            SplineIndex = splineIndex;
            StartingKnot = startingKnot;
            KnotsCount = knotsCount;
            Direction = direction;
        }

        public override string ToString()
        {
            return $"SplineIndex {SplineIndex} StartingKnot {StartingKnot} Count {KnotsCount}";
        }
    }
}