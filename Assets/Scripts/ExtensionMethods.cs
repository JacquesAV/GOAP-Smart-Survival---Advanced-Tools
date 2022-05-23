using System.Collections.Generic;
using System.Linq;
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
        // Temporary float to track accumalated distance.
        float remainingDistance = 0;

        // Iterate over each corner and calculate the remaining distance.
        for (int i = 0; i < pathCorners.Length - 1; ++i)
        {
            remainingDistance += Vector3.Distance(pathCorners[i], pathCorners[i + 1]);
        }

        return remainingDistance;
    }

    /// <summary>
    /// Convert a given list into a queue.
    /// </summary>
    /// <param name="givenList">The list being converted.</param>
    /// <returns>The converted list to queue.</returns>
    public static Queue<Object> ConvertListToQueue(List<Object> givenList)
    {
        // Declare a new queue and add to it.
        var newQueue = new Queue<Object>();
        for (int i = 0; i < givenList.Count; i++) newQueue.Enqueue(givenList[i]);
        return newQueue;
    }

    /// <summary>
    /// Convert a given list into a queue.
    /// </summary>
    /// <typeparam name="T">Generid type for the list content.</typeparam>
    /// <param name="givenList">The list being converted.</param>
    /// <returns>The converted list to queue.</returns>
    public static Queue<T> ConvertListToQueue<T>(this IList<T> givenList)
    {
        // Declare a new queue and add to it.
        var newQueue = new Queue<T>();
        for (int i = 0; i < givenList.Count; i++) newQueue.Enqueue(givenList[i]);
        return newQueue;
    }

    /// <summary>
    /// Shuffles the element order of the specified list.
    /// Smooth Foundations Method.
    /// </summary>
    /// <typeparam name="T">Generid type for the list content.</typeparam>
    /// <param name="ts">The list being shuffled.</param>
    public static void Shuffle<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }

    /// <summary>
    /// Populates a list with the same value.
    /// </summary>
    /// <typeparam name="T">The type being used in the list.</typeparam>
    /// <param name="givenList">The list being iterated over.</param>
    /// <param name="givenValue">The default value for the list.</param>
    /// <param name="givenCount">The amount to populate the list with.</param>
    public static void Populate<T>(this List<T> givenList, T givenValue, int givenCount)
    {
        for (int i = 0; i < givenCount; i++)
        {
            givenList.Add(givenValue);
        }
    }

    /// <summary>
    /// Populates an array with the same value.
    /// </summary>
    /// <typeparam name="T">The type being used in the list.</typeparam>
    /// <param name="givenArray">The list being iterated over.</param>
    /// <param name="givenValue">The default value for the list.</param>
    public static void Populate<T>(this T[] givenArray, T givenValue)
    {
        for (int i = 0; i < givenArray.Length; i++)
        {
            givenArray[i] = givenValue;
        }
    }

    /// <summary>
    /// Enum that represents quadrants.
    /// </summary>
    public enum Quadrant 
    {
        Undefined = 0,
        UpperRight,
        UpperLeft,
        LowerLeft,
        LowerRight
    }
}
