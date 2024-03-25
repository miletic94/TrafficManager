using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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
        Debug.ClearDeveloperConsole();
        splineAnimate = GetComponent<SplineAnimate>();

        SplineSlice<Spline> slice0 = new SplineSlice<Spline>(roadSplineContainer.Splines[0], new SplineRange(0, 2));
        SplineSlice<Spline> slice1 = new SplineSlice<Spline>(roadSplineContainer.Splines[1], new SplineRange(1, 2));
        SplineSlice<Spline> slice2 = new SplineSlice<Spline>(roadSplineContainer.Splines[2], new SplineRange(1, 2));
        SplineSlice<Spline> slice3 = new SplineSlice<Spline>(roadSplineContainer.Splines[3], new SplineRange(2, 2));

        SplinePath splinePath = new SplinePath(new SplineSlice<Spline>[4] { slice0, slice1, slice2, slice3 });

        IReadOnlyList<SplineKnotIndex> knotLinkCollection = roadSplineContainer.KnotLinkCollection.GetKnotLinks(new SplineKnotIndex(0, 0));
        foreach (SplineKnotIndex ski in knotLinkCollection)
        {
            Debug.Log(ski);
        }
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
