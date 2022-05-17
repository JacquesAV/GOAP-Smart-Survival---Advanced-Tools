using UnityEngine;

/// <summary>
/// Action where an agent is able to collect food.
/// </summary>
public class CollectFood : GAction
{
    /// <summary>
    /// How fast the agent is able to perform the action.
    /// </summary>
    [Range(1, 10)]
    public int collectionRate = 1;

    /// <summary>
    /// Pre perform, where any the action is verified.
    /// </summary>
    /// <returns>If successfully pre performed.</returns>
    public override bool PrePerform()
    {
        // Try and find food on the map.
        target = GetClosestTarget(GWorld.foodPoints);

        // If no target or at capacity, then fail to begin action.
        if (target == null || HasReachedCapacity())
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Post perform, where any results of the action are applied to the agent.
    /// </summary>
    /// <returns>If successfully post performed.</returns>
    public override bool PostPerform()
    {
        // Add food to the backpack.
        gAgent.inventory.AddFood(1);

        // Check if at capacity.
        if (HasReachedCapacity())
        {
            // Declare that backpack is full.
            agentBeliefs.ModifyState("ReachedFoodCapacity", 1);
        }

        // Add belief that they have food.
        agentBeliefs.ModifyState("HasFood", 1);

        // Request object destruction.
        GWorld.Instance.RemoveFoodPoint(target.gameObject, true);

        return true;
    }

    /// <summary>
    /// Checks if the agent has reached any food capacity.
    /// </summary>
    /// <returns>If the agent backpack is full.</returns>
    public bool HasReachedCapacity()
    {
        // Temporary inventory reference.
        GInventory inventory = gAgent.inventory;

        // Check if backpack returns as full.
        return inventory.HasReachedCapacity(inventory.TotalFood, inventory.foodCapacity);
    }
}
