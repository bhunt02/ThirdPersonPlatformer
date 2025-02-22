
using UnityEngine;

public class Utilities
{
    public static Vector3 CubicEaseIn(Vector3 end, Vector3 start, float target)
    {
        end -= start;
        return end * Mathf.Pow(target,3) + start;
    }

    public static Vector3 CubicEaseOut(Vector3 end, Vector3 start, float target)
    {
        target--;
        end -= start;
        return end * (Mathf.Pow(target, 3) + 1) + start;
    }
}