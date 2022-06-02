using UnityEngine;

/// <summary>
/// Action where an agent returns to their home.
/// </summary>
public class ReturnHome : GAction
{
    /// <summary>
    /// Pre perform, where any the action is verified.
    /// </summary>
    /// <returns>If successfully pre performed.</returns>
    public override bool PrePerform()
    {
        // Try and find food on the map.
        target = GetClosestAvailableActionPointTarget(GWorld.homePoints);

        // If no target then fail to begin action.
        if (target == null)
        {
            return false;
        }

        // Get the action point.
        targetActionPoint = target.GetComponent<ActionPoint>();

        // Check and reserve if the action point allows for early reservation.
        if (targetActionPoint.GlobalAllowance)
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
        // Inform the manager of the agents return, throwing a warning if one doesnt exist.
        if(SurvivalSimulationManager.SingletonManager)
        {
            SurvivalSimulationManager.SingletonManager.DebugFood(gAgent.inventory.TotalFood);
        }
        else
        {
            Debug.LogWarning("No simulation manager found!");
        }

        // Set the belief that the agent has returned home.
        agentBeliefs.SetState("ReturnedHome", 1);

        // Clear inventory of food.
        //gAgent.inventory.ClearFood();

        // Remove belief that the agent is at food capacity.
        //agentBeliefs.RemoveState("ReachedFoodCapacity");

        // Remove belief that the agent has food.
        //agentBeliefs.RemoveState("HasFood");

        return true;
    }
}
