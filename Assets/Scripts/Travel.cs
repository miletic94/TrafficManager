using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

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

        ShortestPath(roadSplineContainer, new SplineKnotIndex(1, 1), new SplineKnotIndex(3, 3));

        // quaternion quat = splinePath[2].Rotation;
        // quaternion conj = math.conjugate(quat);
        // Matrix4x4 rotationMatrix4x4 = Matrix4x4.Rotate(conj);
        // Vector3 tangentOut = splinePath[1].TangentOut;

        // Vector3 rotateTangentOut = rotationMatrix4x4.MultiplyPoint(tangentOut);
        // Debug.Log(rotateTangentOut);

        Spline newSpline = new Spline
        {
            splinePath[0],
            new BezierKnot(splinePath[1].Position, splinePath[1].TangentIn, RotateVector(splinePath[1].Rotation, splinePath[2].Rotation, splinePath[2].TangentOut), splinePath[1].Rotation),
            new BezierKnot(splinePath[3].Position, splinePath[3].TangentIn, RotateVector(splinePath[3].Rotation, splinePath[4].Rotation, splinePath[4].TangentOut), splinePath[3].Rotation),
            new BezierKnot(splinePath[5].Position, splinePath[5].TangentIn, RotateVector(splinePath[5].Rotation, splinePath[6].Rotation, splinePath[6].TangentOut), splinePath[5].Rotation),
            new BezierKnot(splinePath[7].Position, splinePath[7].TangentIn, RotateVector(splinePath[7].Rotation, splinePath[8].Rotation, splinePath[8].TangentOut), splinePath[7].Rotation),
        };

        // Debug.Log(RotateVector(splinePath[0].Rotation, quaternion.identity, splinePath[0].TangentIn));
        // Debug.Log(RotateVector(splinePath[1].Rotation, quaternion.identity, splinePath[1].TangentIn));

        // Debug.Log(CurveUtility.CalculateLength(new BezierCurve(splinePath[0].Position, RotateVector(splinePath[0].Rotation, quaternion.identity, splinePath[0].TangentOut), RotateVector(splinePath[1].Rotation, quaternion.identity, splinePath[1].TangentIn), splinePath[1].Position), 4));

        // BezierKnot firstKnot = roadSplineContainer.Splines[0][0];
        // BezierKnot secondKnot = roadSplineContainer.Splines[0][1];
        // Vector3 firstKnotTangentOutPos = TangentWorldPosition(firstKnot, TangentType.TangentOut);
        // Vector3 secondKnotTangentInPos = TangentWorldPosition(secondKnot, TangentType.TangentIn);
        // Debug.Log(CurveUtility.CalculateLength(new BezierCurve(firstKnot.Position, TangentWorldPosition(firstKnot, TangentType.TangentOut), TangentWorldPosition(secondKnot, TangentType.TangentIn), secondKnot.Position)));
        Debug.Log(roadSplineContainer.Splines.Count);
        Debug.Log(roadSplineContainer.Splines[0].Count);
        for (int i = 0; i < roadSplineContainer.Splines.Count; i++)
        {
            for (int j = 0; j < roadSplineContainer[i].Count - 1; j++)
            {
                BezierKnot firstKnot = roadSplineContainer.Splines[i][j];
                BezierKnot secondKnot = roadSplineContainer.Splines[i][j + 1];
                var distance = CurveUtility.CalculateLength(new BezierCurve(firstKnot.Position, TangentWorldPosition(firstKnot, TangentType.TangentOut), TangentWorldPosition(secondKnot, TangentType.TangentIn), secondKnot.Position));
                Debug.Log($"SplineKnot {i}, {j} SplineKnot {i}, {j + 1} Distance {distance}");
            }
        }

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

    Vector3 TangentWorldPosition(BezierKnot knot, TangentType tangentType)
    {
        float3 tangentPosition = tangentType == TangentType.TangentIn ? knot.TangentIn : knot.TangentOut;
        // TODO: Why this works and RotateVector(knto.Rotation, quaternion.identity, tangetnPosition) doesn't?
        return (Vector3)knot.Position + RotateVector(quaternion.identity, knot.Rotation, tangentPosition);
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
        startNode.SetCost(splineContainer, endNode);
        endNode.SetCost(splineContainer, startNode, endNode);

        HashSet<AStarNode> visited = new HashSet<AStarNode>();
        //TODO: Implement heap
        SortedSet<AStarNode> heap = new SortedSet<AStarNode>()
        {
            startNode
        };
        while (heap.Count > 0)
        {
            AStarNode current = heap.First();
            // Debug.Log($"CURRENT: {current}");
            // Debug.Log($"END: {endNode}");

            if (current.Equals(endNode))
            {
                // ShowVisited(visited);
                return;
            };

            heap.Remove(current);
            visited.Add(current);
            HashSet<AStarNode> neighbors = current.GetNeighbors(splineContainer);
            foreach (AStarNode n in neighbors)
            {
                // 1. Calculate cost
                n.SetCost(splineContainer, startNode, endNode);
                // 2. If neighbor isn't in visited add neighbor to heap.
                // TODO: If neighbor is nearer when it comes from this parent then from the previous, change it's parent and fCost, gCost and hCost;
                if (!visited.Contains(n))
                {
                    heap.Add(n);
                }
            }
        }

        // foreach (AStarNode n in heap)
        // {
        //     Debug.Log($"HEAP: {n}");
        // }
    }
    void ShowVisited(HashSet<AStarNode> visited)
    {
        // visited.Add(new AStarNode(splineContainer.KnotLinkCollection.GetKnotLinks(new SplineKnotIndex(1, 2))));
        foreach (AStarNode n in visited)
        {
            Debug.Log($"VISITED: {n}");
        }
    }
    Vector3 RotateVector(quaternion fromQuaternion, quaternion toQuaternion, Vector3 vector)
    {
        quaternion q1 = fromQuaternion;
        quaternion q2 = toQuaternion;
        quaternion q1conj = math.conjugate(q1);

        quaternion q = math.mul(q2, q1conj);
        Matrix4x4 rotation = Matrix4x4.Rotate(q);

        return rotation.MultiplyPoint(vector);
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