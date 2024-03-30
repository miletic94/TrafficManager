using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Splines;
using System.Linq;

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


        splineContainer.Splines = new Spline[1] { newSpline };
        splineAnimate.Container = splineContainer;
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

        HashSet<AStarNode> neighbors = startNode.GetNeighbors(splineContainer);

        SortedSet<AStarNode> visited = new SortedSet<AStarNode>();
        //TODO: Implement heap
        List<AStarNode> heap = new List<AStarNode>();
        foreach (AStarNode n in neighbors)
        {
            // 1. Calculate cost
            n.SetCost(splineContainer, startNode, endNode);
            // 2. If it isn't in visited add neighbor to heap.
            Debug.Log($"n: {n}; hash: {n.GetHashCode()} visited: {visited.Contains(n)}");
            if (!visited.Contains(n))
            {
                heap.Add(n);
                visited.Add(n);
            }
        }

        // visited.Add(new AStarNode(splineContainer.KnotLinkCollection.GetKnotLinks(new SplineKnotIndex(1, 2))));
        // foreach (AStarNode n in visited)
        // {
        //     // Debug.Log(n.ToString());
        // }
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