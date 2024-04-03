using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using System;

public static class Utils
{
    public enum TangentType
    {
        TangentIn,
        TangentOut
    }
    // Probably redundant 
    // TODO: (Try by creating BezierCurve with BezierKnots as arguments.)
    public static Vector3 TangentWorldPosition(BezierKnot knot, TangentType tangentType)
    {
        float3 tangentPosition = tangentType == TangentType.TangentIn ? knot.TangentIn : knot.TangentOut;
        // TODO: Why this works and RotateVector(knto.Rotation, quaternion.identity, tangetnPosition) doesn't?
        return (Vector3)knot.Position + RotateVector(quaternion.identity, knot.Rotation, tangentPosition);
    }

    public static Vector3 RotateVector(quaternion fromQuaternion, quaternion toQuaternion, Vector3 vector)
    {
        quaternion q1 = fromQuaternion;
        quaternion q2 = toQuaternion;
        quaternion q1conj = math.conjugate(q1);

        quaternion q = math.mul(q2, q1conj);
        Matrix4x4 rotation = Matrix4x4.Rotate(q);

        return rotation.MultiplyPoint(vector);
    }

    public static void OrderSKIsByKnot(SplineKnotIndex x, SplineKnotIndex y, out SplineKnotIndex first, out SplineKnotIndex second)
    {
        if (x.Spline != y.Spline)
        {
            throw new Exception("Not same splines");
        }
        if (x.Knot <= y.Knot)
        {
            first = x;
            second = y;
        }
        else
        {
            first = y;
            second = x;
        }
    }
}



