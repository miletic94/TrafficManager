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

    /// <summary>
    /// Rotate a vector from its orientation specified by a starting quaternion to a new orientation specified by a target quaternion.
    /// </summary>
    /// <param name="fromQuaternion">The starting rotation quaternion.</param>
    /// <param name="toQuaternion">The target rotation quaternion.</param>
    /// <param name="vector">The vector to be rotated.</param>
    /// <returns>The rotated vector.</returns>
    public static Vector3 RotateVector(quaternion fromQuaternion, quaternion toQuaternion, Vector3 vector)
    {
        // Get the rotation from "fromQuaternion" to "toQuaternion"
        Quaternion rotation = toQuaternion * Quaternion.Inverse(fromQuaternion);

        // Rotate the vector using the calculated rotation
        return rotation * vector;
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



