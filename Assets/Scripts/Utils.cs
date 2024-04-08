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

    public delegate float DistanceCalculatorDelegate(SplineContainer container, SplineKnotIndex fromSKI, SplineKnotIndex toSKI, bool approximate = false, int resolution = 30);

    // Use approximate to switch algorithm by which curve length is calculated.
    // approximate = false will use CurveUtility.CalculateLength which is more precise, but slower. Here resolution parameter is used as precision measure.
    // approximate = true will use CurveUtility.ApproximateLength which is faster, but less precise.
    public static float GetDistanceBySKIConnections(SplineContainer container, SplineKnotIndex fromSKI, SplineKnotIndex toSKI, bool approximate = false, int resolution = 30)
    {
        if (fromSKI.Equals(toSKI))
        {
            return 0;
        }

        if (!AreSKINeighbors(fromSKI, toSKI))
        {
            throw new Exception($"{fromSKI} and {toSKI} are not connected");
        }

        if (!IsXBeforeYByKnot(fromSKI, toSKI)) throw new ArgumentException($"{nameof(fromSKI)} and {nameof(toSKI)} must be ordered");

        BezierKnot fromKnot = container.Splines[fromSKI.Spline][fromSKI.Knot];
        BezierKnot toKnot = container.Splines[toSKI.Spline][toSKI.Knot];
        BezierCurve curve = new BezierCurve(fromKnot.Position, toKnot.Position);

        if (approximate)
        {
            return CurveUtility.ApproximateLength(curve);
        }
        return CurveUtility.CalculateLength(curve, resolution);
    }

    static bool IsXBeforeYByKnot(SplineKnotIndex x, SplineKnotIndex y)
    {
        if (x.Spline != y.Spline || x.Knot > y.Knot) return false;
        return true;
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

    public static bool AreSKINeighbors(SplineKnotIndex x, SplineKnotIndex y)
    {
        if (x.Spline != y.Spline)
        {
            return false;
        }
        return x.Knot == y.Knot + 1 || x.Knot == y.Knot - 1;
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



