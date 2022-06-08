using System;
using UnityEngine;

public static class Math2
{
    public static Vector3[] directions = new[] { Vector3.left, Vector3.right};
    public static Vector3 Devide(Vector3 a, Vector3 b) => 
        new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    public static Vector3 Abs(Vector3 a) => new Vector3(Mathf.Abs(a.x), Mathf.Abs(a.y), Mathf.Abs(a.z));
    public static Vector3 Min(Vector3 a, Vector3 b) => new Vector3(Min(a.x, b.x), Min(a.y, b.y), Min(a.z, b.z));
    private static float Min(float ax,float bx)
    {
        
        return Mathf.Abs(ax)<Mathf.Abs(bx)?ax:bx;
    }
    public static Rect rect = new Rect(0,0,1,1);
    public static Vector2 half= new Vector2(.5f,.5f);
    public const float itchToM= 0.0254f; 
    public static Vector3 Lerp(Vector3 a, Vector3 b, Vector3 t)
    {
        return new Vector3(a.x + (b.x - a.x) * t.x, a.y + (b.y - a.y) * t.y, a.z + (b.z - a.z) * t.z);
    }
    public static float MaxZero(float f)
    {
        return Math.Max(0.00001f, f);
    }

    

    public const int MinValue = -99999;
    public const int MaxValue = 99999;
    public static bool XAnd(bool a, bool b)
    {
        return !(a ^ b);
    }

    public static bool XOr(bool a, bool b)
    {
        return (a ^ b);
    }
    public static float Mod(float a, float n)
    {
        return ((a % n) + n) % n;
    }
    public static int Mod(int a, int n)
    {
        return ((a % n) + n) % n;
    }
    public static int IntPow(int x, int pow)
    {
        int ret = 1;
        while (pow != 0)
        {
            if ((pow & 1) == 1)
                ret *= x;
            x *= x;
            pow >>= 1;
        }
        return ret;
    }
    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Mathf.Clamp01(Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB));
    }
    
    public static Vector3 MoveTowardsUnclamped(
        Vector3 current,
        Vector3 target,
        float maxDistanceDelta)
    {
        float num1 = target.x - current.x;
        float num2 = target.y - current.y;
        float num3 = target.z - current.z;
        float num4 = (float) ((double) num1 * (double) num1 + (double) num2 * (double) num2 + (double) num3 * (double) num3);
        if ((double) num4 == 0.0)
            return target;
        float num5 = (float) Math.Sqrt((double) num4);
        return new Vector3(current.x + num1 / num5 * maxDistanceDelta, current.y + num2 / num5 * maxDistanceDelta, current.z + num3 / num5 * maxDistanceDelta);
    }
    public static bool oppositeSigns(float x, float y) 
    { 
        return (x < 0)? (y > 0): (y < 0); 
    }
    // public static Quaternion FromToRotation(Quaternion fr,Quaternion to)
    // {
    //     return to * Quaternion.Inverse(fr);
    // }
    public static Quaternion FromToRotation(Quaternion from, Quaternion to)
    {
        return Quaternion.Inverse(from) * to;
    }
  
}