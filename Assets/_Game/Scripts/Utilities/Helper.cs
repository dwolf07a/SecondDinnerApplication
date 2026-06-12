using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Helper
{
    public static Vector3 SmoothStep(Vector3 initial, Vector3 target, float t)
    {
        return new Vector3(
            Mathf.SmoothStep(initial.x, target.x, t),
            Mathf.SmoothStep(initial.y, target.y, t),
            Mathf.SmoothStep(initial.z, target.z, t)
            );
    }

    public static Vector3 RandomRange(Vector3 min, Vector3 max)
    {
        return new Vector3(
            UnityEngine.Random.Range(min.x, max.x),
            UnityEngine.Random.Range(min.y, max.y),
            UnityEngine.Random.Range(min.z, max.z)
            );
    }

    public static Vector3 RandomVectorInCone(Vector3 direction, float angleDegrees)
    {
        // Convert angle to radians
        float angleRadians = angleDegrees * Mathf.Deg2Rad;

        // Generate a random point on the unit circle's circumference
        float t = UnityEngine.Random.Range(0f, 2 * Mathf.PI);
        float u = UnityEngine.Random.Range(0f, 1f);

        // Determine the radius of the cone's base at this 'u'
        float radius = u * Mathf.Tan(angleRadians);

        // Calculate the offset from the cone's central axis
        float x = radius * Mathf.Cos(t);
        float z = radius * Mathf.Sin(t);
        Vector3 offset = new Vector3(x, 0, z);

        // Rotate the offset vector to align with the cone's direction
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);
        offset = rotation * offset;

        // Return the final vector: original direction + offset
        return direction + offset;
    }

    public static bool RaySphereIntersect(Vector3 rayStart, Vector3 rayDir, Vector3 spherePoint, float sphereRadius)
    {
        if (rayDir.sqrMagnitude <= Mathf.Epsilon || sphereRadius < Mathf.Epsilon)
            return false;

        rayDir.Normalize();

        // Calculate ray start's offset from the sphere center
        Vector3 p = rayStart - spherePoint;

        float rSquared = sphereRadius * sphereRadius;
        float p_d = Vector3.Dot(p, rayDir);

        // The sphere is behind or surrounding the start point.
        if (p_d > 0 || Vector3.Dot(p, p) < rSquared)
            return false;

        // Flatten p into the plane passing through c perpendicular to the ray.
        // This gives the closest approach of the ray to the center.
        Vector3 a = p - p_d * rayDir;

        float aSquared = Vector3.Dot(a, a);

        // Closest approach is outside the sphere.
        if (aSquared > rSquared)
            return false;

        //We hit the sphere
        return true;
    }

    public static GameObject[] ShuffleList(GameObject[] myList, int seed = -1)
    {
        if (seed == -1)
            seed = (int)DateTime.Now.ToFileTime();
        UnityEngine.Random.State oldState = UnityEngine.Random.state;
        UnityEngine.Random.InitState(seed);
        
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < myList.Length; t++)
        {
            GameObject tmp = myList[t];
            int r = UnityEngine.Random.Range(t, myList.Length);
            myList[t] = myList[r];
            myList[r] = tmp;
        }

        UnityEngine.Random.state = oldState;

        return myList;
    }

    public static ArrayList ShuffleList(ArrayList myList, int seed = -1)
    {
        if (seed == -1)
            seed = (int)DateTime.Now.ToFileTime();
        UnityEngine.Random.State oldState = UnityEngine.Random.state;
        UnityEngine.Random.InitState(seed);

        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < myList.Count; t++)
        {
            object tmp = myList[t];
            int r = UnityEngine.Random.Range(t, myList.Count);
            myList[t] = myList[r];
            myList[r] = tmp;
        }

        UnityEngine.Random.state = oldState;

        return myList;
    }

    public static bool IsInCameraBounds(Camera c, Vector3 worldPosition)
    {
        if (c == null)
            return false;

        // Convert world position to viewport point
        Vector3 viewportPoint = c.WorldToViewportPoint(worldPosition);

        // Check if the point is within the viewport (0 to 1 for x and y) and in front of the camera (z > 0)
        bool inBounds = viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                        viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
                        viewportPoint.z > 0;

        return inBounds;
    }
}