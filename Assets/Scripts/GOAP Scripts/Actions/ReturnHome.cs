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
        // If no target exists, then fail to begin action.
        if (target == null)
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

        // Clear inventory of food.
        gAgent.inventory.ClearFood();

        // Set the belief that the agent has returned home.
        agentBeliefs.SetState("ReturnedHome", 1);

        // Remove belief that the agent is at food capacity.
        agentBeliefs.RemoveState("ReachedFoodCapacity");

        // Remove belief that the agent has food.
        agentBeliefs.RemoveState("HasFood");

        return true;
    }
}
