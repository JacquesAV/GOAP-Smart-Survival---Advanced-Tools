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
        target = GetClosestAvailableActionPointTarget(GWorld.foodPoints);

        // If no target or at capacity, then fail to begin action.
        if (target == null || HasReachedCapacity())
        {
            return false;
        }

        // Get the action point.
        targetActionPoint = target.GetComponent<ActionPoint>();

        // Check and reserve if the action point allows for early reservation.
        if(targetActionPoint.GlobalAllowance)
        {
            targetActionPoint.ReserveActionPoint(gAgent);
        }

        return true;
    }

    /// <summary>
    /// Intra perform, where checks are made during the course of traveling to an action.
    /// </summary>
    /// <returns>If successfully intra performed.</returns>
    public override bool IntraPerform()
    {
        // Check if the action point still exists.
        if (target == null || targetActionPoint == null)
        {
            Debug.Log("Action point dissapeared too early!");
            return false;
        }

        // Check if action point has no space and is not reserved by self.
        if (!targetActionPoint.HasReservedActionPoint(gAgent) && !targetActionPoint.CheckForSpace())
        {
            return false;
        }

        // Check if action point is dynamically moving and update destination.
        if (targetActionPoint.IsDynamicallyMoving)
        {
            navAgent.SetDestination(target.transform.position);
        }

        // Check if action point is still accessible.
        if (!gAgent.IsPathPending && !gAgent.HasPath)
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
