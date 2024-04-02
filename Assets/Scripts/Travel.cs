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
    // Start is called before the first frame update
    void Awake()
    {

        // SplineKnot: {0, 0}, [{0, 1}, {1, 1}], [{1, 2}, {2, 1}], [{2, 2}, {3, 2}], {3, 3}
        Debug.ClearDeveloperConsole();
        splineAnimate = GetComponent<SplineAnimate>();

        SplineSlice<Spline> slice0 = new SplineSlice<Spline>(roadSplineContainer.Splines[0], new SplineRange(0, 2));
        SplineSlice<Spline> slice1 = new SplineSlice<Spline>(roadSplineContainer.Splines[1], new SplineRange(1, 2));
        SplineSlice<Spline> slice2 = new SplineSlice<Spline>(roadSplineContainer.Splines[2], new SplineRange(1, 2));
        SplineSlice<Spline> slice3 = new SplineSlice<Spline>(roadSplineContainer.Splines[3], new SplineRange(2, 2));

        SplinePath splinePath = new SplinePath(new SplineSlice<Spline>[4] { slice0, slice1, slice2, slice3 });

        ShortestPath(roadSplineContainer, new SplineKnotIndex(3, 0), new SplineKnotIndex(2, 0));

        Spline newSpline = new Spline
        {
            splinePath[0],
            new BezierKnot(splinePath[1].Position, splinePath[1].TangentIn, Utils.RotateVector(splinePath[1].Rotation, splinePath[2].Rotation, splinePath[2].TangentOut), splinePath[1].Rotation),
            new BezierKnot(splinePath[3].Position, splinePath[3].TangentIn, Utils.RotateVector(splinePath[3].Rotation, splinePath[4].Rotation, splinePath[4].TangentOut), splinePath[3].Rotation),
            new BezierKnot(splinePath[5].Position, splinePath[5].TangentIn, Utils.RotateVector(splinePath[5].Rotation, splinePath[6].Rotation, splinePath[6].TangentOut), splinePath[5].Rotation),
            new BezierKnot(splinePath[7].Position, splinePath[7].TangentIn, Utils.RotateVector(splinePath[7].Rotation, splinePath[8].Rotation, splinePath[8].TangentOut), splinePath[7].Rotation),
        };

        // Debug.Log(RotateVector(splinePath[0].Rotation, quaternion.identity, splinePath[0].TangentIn));
        // Debug.Log(RotateVector(splinePath[1].Rotation, quaternion.identity, splinePath[1].TangentIn));

        // Debug.Log(CurveUtility.CalculateLength(new BezierCurve(splinePath[0].Position, RotateVector(splinePath[0].Rotation, quaternion.identity, splinePath[0].TangentOut), RotateVector(splinePath[1].Rotation, quaternion.identity, splinePath[1].TangentIn), splinePath[1].Position), 4));

        // BezierKnot firstKnot = roadSplineContainer.Splines[0][0];
        // BezierKnot secondKnot = roadSplineContainer.Splines[0][1];
        // Vector3 firstKnotTangentOutPos = TangentWorldPosition(firstKnot, TangentType.TangentOut);
        // Vector3 secondKnotTangentInPos = TangentWorldPosition(secondKnot, TangentType.TangentIn);
        // Debug.Log(CurveUtility.CalculateLength(new BezierCurve(firstKnot.Position, TangentWorldPosition(firstKnot, TangentType.TangentOut), TangentWorldPosition(secondKnot, TangentType.TangentIn), secondKnot.Position)));

        // Debug.Log(roadSplineContainer.Splines.Count);
        // Debug.Log(roadSplineContainer.Splines[0].Count);

        // for (int i = 0; i < roadSplineContainer.Splines.Count; i++)
        // {
        //     for (int j = 0; j < roadSplineContainer[i].Count - 1; j++)
        //     {
        //         BezierKnot firstKnot = roadSplineContainer.Splines[i][j];
        //         BezierKnot secondKnot = roadSplineContainer.Splines[i][j + 1];
        //         BezierCurve curve = new BezierCurve(firstKnot.Position, Utils.TangentWorldPosition(firstKnot, Utils.TangentType.TangentOut), Utils.TangentWorldPosition(secondKnot, Utils.TangentType.TangentIn), secondKnot.Position);
        //         var distance = CurveUtility.CalculateLength(curve);
        //         Debug.Log($"SplineKnot {i}, {j} SplineKnot {i}, {j + 1} Distance {distance}");
        //     }
        // }

        // Debug.Log($"First Knot Position: {firstKnot.Position}");
        // Debug.Log($"First Knot Rotation: {firstKnot.Rotation}");
        // Debug.Log($"First Knot TangentOut: {firstKnot.TangentOut}");
        // Debug.Log($"Rotated: {RotateVector(firstKnot.Rotation, quaternion.identity, firstKnot.TangentOut)}");
        // Debug.Log($"First Knot TangentOut Position: {firstKnotTangentOutPos}");

        // Debug.Log(secondKnot.Position);
        // Debug.Log(secondKnot.Rotation);
        // Debug.Log(secondKnot.TangentIn);
        // Debug.Log(secondKnotTangentInPos);

        // Debug.Log(CurveUtility.ApproximateLength(new BezierCurve(firstKnot.Position, RotateVector(firstKnot.Rotation, quaternion.identity, firstKnot.TangentOut), RotateVector(secondKnot.Rotation, quaternion.identity, secondKnot.TangentIn), secondKnot.Position)));

        // Debug.Log(CurveUtility.CalculateLength(new BezierCurve(firstKnot.Position, TangentWorldPosition(firstKnot, TangentType.TangentOut), TangentWorldPosition(secondKnot, TangentType.TangentIn), secondKnot.Position)));

        // Debug.Log(CurveUtility.CalculateLength(new BezierCurve(firstKnot.Position, secondKnot.Position), 4));

        // TestRotations();
        splineContainer.Splines = new Spline[1] { newSpline };
        splineAnimate.Container = splineContainer;
    }



    // void TestRotations()
    // {
    //     quaternion a0 = new quaternion(0, 0, 0, 1);
    //     quaternion a90 = new quaternion(0.71f, 0, 0, 0.71f);
    //     quaternion a180 = new quaternion(1, 0, 0, 0);
    //     quaternion a270 = new quaternion(0.71f, 0, 0, -0.71f);

    //     Vector3 vector = new Vector3(0, 0, 5);

    //     List<quaternion> angles = new List<quaternion>()
    //     { a0, a90, a180, a270};

    //     foreach (quaternion angle in angles)
    //     {
    //         Debug.Log(RotateVector(angle, quaternion.identity, vector));
    //     }
    // }

    public enum TangentType
    {
        TangentIn,
        TangentOut
    }

    private void Start()
    {
        // splineAnimate.Play();
    }

    // void ShortestPath(SplineKnotIndex start, SplineKnotIndex end)
    // {
    //     BezierKnot endKnot = roadSplineContainer[end.Spline][end.Knot];
    //     SortedSet<SplineKnotIndexWithDistance> prioritySet = new SortedSet<SplineKnotIndexWithDistance>();
    //     IReadOnlyList<SplineKnotIndex> knotLinkCollection = roadSplineContainer.KnotLinkCollection.GetKnotLinks(start);
    //     SortedSet<ComparableSplineKnotIndex> visited = new SortedSet<ComparableSplineKnotIndex>();
    // foreach (SplineKnotIndex ski in knotLinkCollection)
    //     {
    //         //TODO: raščlani ove špagete
    //         Spline spline = roadSplineContainer[ski.Spline];

    //         int next = ski.Knot + 1;
    //         int prev = ski.Knot - 1;
    //         visited.Add(new ComparableSplineKnotIndex(start));
    //         Debug.Log(visited.Contains(new ComparableSplineKnotIndex(start)));
    //         if (next < spline.Count)
    //         {
    //             BezierKnot nextKnot = spline[next];
    //             SplineKnotIndex nextSplineKnotIndex = new SplineKnotIndex(ski.Spline, next);
    //             float nextDistance = Vector3.Distance(nextKnot.Position, endKnot.Position);
    //             prioritySet.Add(new SplineKnotIndexWithDistance(nextSplineKnotIndex, nextDistance));
    //         }

    //         if (prev > -1)
    //         {
    //             BezierKnot prevKnot = spline[prev];
    //             SplineKnotIndex prevSplineKnotIndex = new SplineKnotIndex(ski.Spline, prev);
    //             float prevDistance = Vector3.Distance(prevKnot.Position, endKnot.Position);
    //             prioritySet.Add(new SplineKnotIndexWithDistance(prevSplineKnotIndex, prevDistance));
    //         }
    //     }

    // foreach (SplineKnotIndexWithDistance skid in prioritySet)
    // {
    //     Debug.Log($"Spline {skid.SplineKnotIndex.Spline} Knot {skid.SplineKnotIndex.Knot} Distance: {skid.Distance}");
    // }
    // }

    void ShortestPath(SplineContainer splineContainer, SplineKnotIndex start, SplineKnotIndex end)
    {
        AStarNode startNode = new AStarNode(splineContainer.KnotLinkCollection.GetKnotLinks(start));
        AStarNode endNode = new AStarNode(splineContainer.KnotLinkCollection.GetKnotLinks(end));
        startNode.ComputeAndSetCost(splineContainer, endNode);

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
            // Debug.Log($"CURRENT: {current}");
            // Debug.Log($"END: {endNode}");

            // SplineKnotIndex fromSKI;
            // SplineKnotIndex toSKI;
            // var isFound = current.TryFindKnotLinkToNode(splineContainer, startNode, out fromSKI, out toSKI);
            // if (isFound)
            // {
            //     Debug.Log($"FROMSpline {fromSKI.Spline} FROMKnot {fromSKI.Knot} \n TOSpline {toSKI.Spline} TOKnot {toSKI.Knot}");
            // }
            //
            // SplineKnotIndex fromSKI2;
            // SplineKnotIndex toSKI2;
            // var isFound2 = current.TryFindKnotLinkToNode(splineContainer, endNode, out fromSKI2, out toSKI2);
            // if (isFound2)
            // {
            //     Debug.Log($"FROMSpline {fromSKI2.Spline} FROMKnot {fromSKI2.Knot} \n TOSpline {toSKI2.Spline} TOKnot {toSKI2.Knot}");
            // }
            if (current.Equals(endNode))
            {
                LinkedList<AStarNode> path = GeneratePath(current);
                foreach (AStarNode node in path)
                {
                    Debug.Log(node.ToString());
                }
                return;
            }

            HashSet<AStarNode> neighbors = current.GetNeighbors(splineContainer);
            foreach (AStarNode n in neighbors)
            {

                if (!visited.Contains(n))
                {
                    AStarNode heapN;
                    bool heapContainsN = heap.TryGetValue(n, out heapN);

                    float fCost, gCost, hCost;
                    n.ComputeCost(splineContainer, current, endNode, out gCost, out hCost);
                    fCost = gCost + hCost;

                    if (heapContainsN)
                    {
                        if (heapN.fCost > fCost)
                        {
                            heapN.gCost = gCost;
                            heapN.hCost = hCost;
                            heapN.parent = current;
                        }
                    }
                    else
                    {
                        n.gCost = gCost;
                        n.hCost = hCost;
                        n.parent = current;
                        heap.Add(n);
                    }
                }
            }
        }

        // foreach (AStarNode n in heap)
        // {
        //     Debug.Log($"HEAP: {n}");
        // }
    }

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
    void ShowVisited(HashSet<AStarNode> visited)
    {
        // visited.Add(new AStarNode(splineContainer.KnotLinkCollection.GetKnotLinks(new SplineKnotIndex(1, 2))));
        foreach (AStarNode n in visited)
        {
            Debug.Log($"VISITED: {n}");
        }
    }
}

// class SplineKnotIndexWithDistance : IComparable<SplineKnotIndexWithDistance>
// {
//     SplineKnotIndex k_SplineKnotIndex;
//     float k_Distance;

//     public SplineKnotIndexWithDistance(SplineKnotIndex splineKnotIndex, float distance)
//     {
//         k_SplineKnotIndex = splineKnotIndex;
//         k_Distance = distance;
//     }

//     public SplineKnotIndex SplineKnotIndex => k_SplineKnotIndex;

//     public float Distance => k_Distance;

//     public int CompareTo(SplineKnotIndexWithDistance other)
//     {
//         return this.k_Distance.CompareTo(other.k_Distance);
//     }
// }