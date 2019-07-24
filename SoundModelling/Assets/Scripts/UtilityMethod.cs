using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilityMethod
{

    public static Vector2 ToXZ(this Vector3 v3)
    {
        return new Vector2(v3.x, v3.z);
    }

    public static Vector2 ToXY(this Vector3 v3)
    {
        return new Vector2(v3.x, v3.y);
    }

    public static Vector3 RotateAroundY(float angle, Vector3 baseVector)       //positive = counter-clockwise, angle in radians
    {
        Vector3 baseVector_normal = baseVector.normalized;
        float angleInRadian = AngleToRadian(angle);
        Matrix4x4 rotationMatrixAroundY = new Matrix4x4
                        (new Vector4(Mathf.Cos(angleInRadian), 0, Mathf.Sin(angleInRadian)),
                        new Vector4(0, 1, 0),
                        new Vector4(-Mathf.Sin(angleInRadian), 0, Mathf.Cos(angleInRadian)),
                        new Vector4(0, 0, 0)
                        );
        Vector3 rotatedVector = rotationMatrixAroundY.MultiplyVector(baseVector_normal);
        return rotatedVector;
    }

    public static float AngleToRadian(float angle)
    {
        return angle / 360 * 2 * Mathf.PI;
    }

    public static float RadianToAngle(float radian)
    {
        return radian / 2 / Mathf.PI * 360;
    }

    public static Vector3 VectorProjection(Vector3 from, Vector3 onto)
    {
        return Vector3.Dot(from, onto) / onto.sqrMagnitude * onto;
    }

    public static float VectorProjectionLength(Vector3 from, Vector3 onto)
    {
        return Vector3.Dot(from, onto) / onto.magnitude;
    }
}
