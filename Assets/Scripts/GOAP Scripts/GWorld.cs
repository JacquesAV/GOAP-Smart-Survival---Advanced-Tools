using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Class that tracks globally relevant objects.
/// </summary>
public sealed class GWorld
{
    /// <summary>
    /// Return current instance.
    /// </summary>
    public static GWorld Instance { get; } = new GWorld();

    /// <summary>
    /// Dictionary of all our states.
    /// </summary>
    private static readonly WorldStates world;

    /// <summary>
    /// List of all existing home points.
    /// </summary>
    public static List<GameObject> homePoints = new List<GameObject>();

    /// <summary>
    /// List of all existing food points.
    /// </summary>
    public static List<GameObject> foodPoints = new List<GameObject>();

    /// <summary>
    /// Setup for the world.
    /// </summary>
    static GWorld()
    {
        // Create new dictionary of the states.
        world = new WorldStates();

        // Get all the world home points and modify the state world to report the existing amount.
        homePoints = GameObject.FindGameObjectsWithTag("HomePoint").ToList();
        if (homePoints.Count > 0)
        {
            world.ModifyState("HomePointExists", homePoints.Count);
        }

        // Get all the world delivery points and modify the state world to report the existing amount.
        foodPoints = GameObject.FindGameObjectsWithTag("FoodPoint").ToList();
        if (foodPoints.Count > 0)
        {
            world.ModifyState("FoodPointExists", foodPoints.Count);
        }

        Debug.Log("World");

        // Set the belief of night.
        SetIsNight(false);
    }

    /// <summary>
    /// Remove food point from simulation.
    /// </summary>
    /// <param name="removedObject">The object to remove.</param>
    /// <param name="shouldDestroy">If the object should be destroyed.</param>
    public void RemoveFoodPoint(GameObject removedObject, bool shouldDestroy)
    {
        foodPoints.Remove(removedObject);
        if(shouldDestroy)
        {
            GameObject.Destroy(removedObject);
        }
    }

    /// <summary>
    /// Set the belief of if it is night or day.
    /// </summary>
    /// <param name="isNight">If night should be applied.</param>
    public static void SetIsNight(bool isNight)
    {
        if(isNight)
        {
            world.SetState("IsNight", 1);
        }
        else
        {
            world.RemoveState("IsNight");
        }
    }

    /// <summary>
    /// Constructor for quick creation.
    /// </summary>
    private GWorld() { }

    /// <summary>
    /// Return the states.
    /// </summary>
    /// <returns>The world states.</returns>
    public WorldStates GetWorld()
    {
        return world;
    }
}
