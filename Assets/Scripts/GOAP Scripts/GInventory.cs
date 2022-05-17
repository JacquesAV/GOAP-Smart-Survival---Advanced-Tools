using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The inventory of the agent, tracking resources it has and objects of interest in the world.
/// </summary>
public class GInventory : MonoBehaviour
{
    /// <summary>
    /// Item references saved to the inventory.
    /// </summary>
    public List<GameObject> items = new List<GameObject>();

    /// <summary>
    /// The total amount of food stored.
    /// </summary>
    [field: SerializeField]
    public int TotalFood { get; private set; } = 0;

    /// <summary>
    /// The capacities for food stored.
    /// </summary>
    [Range(1,100)]
    public int foodCapacity = 10;

    /// <summary>
    /// Boolean for if capacity should be used.
    /// </summary>
    public bool usingCapacity = false;

    /// <summary>
    /// Add an item to the inventory.
    /// </summary>
    /// <param name="item">The object being added.</param>
    public void Additem(GameObject item) => items.Add(item);

    /// <summary>
    /// Find an inventory stored object using a tag.
    /// </summary>
    /// <param name="tag">The tag to search for.</param>
    /// <returns>The object that was found.</returns>
    public GameObject FindItemWithTag(string tag)
    {
        foreach(GameObject item in items)
        {
            if(item.tag == tag)
            {
                return item;
            }
        }
        return null;
    }

    /// <summary>
    /// Remove an item.
    /// </summary>
    /// <param name="item">The item being removed.</param>
    public void RemoveItem(GameObject item)
    {
        int indexToRemove = -1;
        foreach(GameObject gameObject in items)
        {
            indexToRemove++;
            if(gameObject == item)
            {
                break;
            }
        }
        if(indexToRemove >= -1)
        {
            items.RemoveAt(indexToRemove);
        }
    }

    /// <summary>
    /// Check if capacity is reached of a specific resource.
    /// </summary>
    /// <param name="current">The current amount stored.</param>
    /// <param name="capacity">The capacity of the resource.</param>
    /// <returns>If capacity has been reached.</returns>
    public bool HasReachedCapacity(int current, int capacity)
    {
        // If at and using capacity, return true.
        if (usingCapacity && current >= capacity)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the excess amount of food that cant be added after adding.
    /// </summary>
    /// <param name="amountToAdd">The amount of being added.</param>
    /// <returns>The excess amount calculated after adding.</returns>
    public int AddFood(int amountToAdd)
    {
        // Add the amount.
        TotalFood += amountToAdd;

        // Excess to calculate.
        int excess = 0;

        // Perform capacit related changes.
        if(usingCapacity)
        {
            // Calculate excess if over food capacity and using limits.
            if (TotalFood > foodCapacity)
            {
                excess = TotalFood - foodCapacity;
            }

            // Clamp the food.
            TotalFood = Mathf.Clamp(TotalFood, 0, foodCapacity);
        }

        // Return excess.
        return excess;
    }

    /// <summary>
    /// Clear the inventory of food being tracked.
    /// </summary>
    public void ClearFood() => TotalFood = 0;
}
