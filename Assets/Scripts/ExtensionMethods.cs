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
    /// <typeparam name="T">Generic type for the list content.</typeparam>
    /// <param name="givenList">The list being shuffled.</param>
    public static void Shuffle<T>(this IList<T> givenList)
    {
        int count = givenList.Count;
        int last = count - 1;

        for (int i = 0; i < last; ++i)
        {
            int randomIndex = UnityEngine.Random.Range(i, count);
            T temporaryItem = givenList[i];
            givenList[i] = givenList[randomIndex];
            givenList[randomIndex] = temporaryItem;
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
    /// A random system class for random number related equations.
    /// </summary>
    private static System.Random randomClass = new System.Random();

    /// <summary>
    /// Returns if a given probablility is succesfull or not.
    /// </summary>
    /// <param name="probability">The probability of this equation.</param>
    /// <returns>If the probability happens.</returns>
    public static bool ProbabilityCheck(double probability) => randomClass.NextDouble() <= probability;
}

/// <summary>
/// Struct that helps define an integer and its boundaries.
/// </summary>
[System.Serializable]
public struct IntMinMax
{
    /// <summary>
    /// The integer value to modify.
    /// </summary>
    public int integer;

    /// <summary>
    /// The minimum value for the integer.
    /// </summary>
    [field: SerializeField]
    public int Minimum { get; private set; }

    /// <summary>
    /// The maximum value for the integer.
    /// </summary>
    [field: SerializeField]
    public int Maximum { get; private set; }

    /// <summary>
    /// Constructor for quick creation of struct.
    /// </summary>
    /// <param name="integer">The integer value to modify.</param>
    /// <param name="Minimum">The minimum value for the integer.</param>
    /// <param name="Maximum">The maximum value for the integer.</param>
    public IntMinMax(int integer, int Minimum, int Maximum)
    {
        this.integer = integer;
        this.Minimum = Minimum;
        this.Maximum = Maximum;
    }
}