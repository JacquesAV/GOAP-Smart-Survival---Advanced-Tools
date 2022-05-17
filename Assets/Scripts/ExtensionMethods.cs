using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that holds useful and non-specific helper methods.
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// Gets the distance of a given list of corners.
    /// </summary>
    /// <param name="pathCorners">The corners being used in distance calculation.</param>
    /// <returns>The distance of the given corners.</returns>
    public static float GetPathDistance(Vector3[] pathCorners)
    {
        //Temporary float to track accumalated distance.
        float remainingDistance = 0;

        //Iterate over each corner and calculate the remaining distance.
        for (int i = 0; i < pathCorners.Length - 1; ++i)
        {
            remainingDistance += Vector3.Distance(pathCorners[i], pathCorners[i + 1]);
        }

        return remainingDistance;
    }
}
