using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager object for an action point and applying limitations upon it.
/// </summary>
public class ActionPoint : MonoBehaviour
{
    /// <summary>
    /// List of agents who are occupying the action point.
    /// </summary>
    [SerializeField]
    private List<GAgent> occupyingAgents = new List<GAgent>();

    /// <summary>
    /// How many agents are capabe of occupying the action point at once.
    /// </summary>
    [SerializeField]
    [Range(0, 50)]
    protected int agentLimitCount = 1;

    /// <summary>
    /// If the action point should restrict the number of agents that can interact with it at a time.
    /// </summary>
    [SerializeField]
    private bool usingAgentLimits;

    /// <summary>
    /// If the action point is dynanically changing positions.
    /// </summary>
    [SerializeField]
    private bool isDynamicallyMoving = false;

    /// <summary>
    /// Getter for dynamic movement.
    /// </summary>
    [HideInInspector]
    public bool IsDynamicallyMoving => isDynamicallyMoving;

    /// <summary>
    /// If the action point only allows agents to reserve it upon arrival.
    /// </summary>
    [SerializeField]
    private bool globalReservation = false;

    /// <summary>
    /// Getter for global allowance.
    /// </summary>
    [HideInInspector]
    public bool GlobalAllowance => globalReservation;

    /// <summary>
    /// Clears the action point of reservations.
    /// </summary>
    public void UnreserveAllActionPoint() => occupyingAgents.Clear();

    /// <summary>
    /// Allows for a given agent to reserve a spot in the action point.
    /// </summary>
    /// <param name="givenAgent">The agent attempting to reserve the point.</param>
    /// <returns>If the action point can be sucessfully assigned.</returns>
    public bool ReserveActionPoint(GAgent givenAgent)
    {
        if(CheckForSpace())
        {
            occupyingAgents.Add(givenAgent);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Allows for a given agent to unreserve a spot in the action point.
    /// </summary>
    /// <param name="givenAgent">The agent unreserving.</param>
    public void UnreserveActionPoint(GAgent givenAgent) => occupyingAgents.Remove(givenAgent);

    /// <summary>
    /// Checks if a given agent has reserved the action point.
    /// </summary>
    /// <param name="givenAgent">The agent being checked.</param>
    /// <returns>If the given agent has reserved the action point.</returns>
    public bool HasReservedActionPoint(GAgent givenAgent) => occupyingAgents.Contains(givenAgent);

    /// <summary>
    /// Checks if the action point has space for agents.
    /// </summary>
    /// <returns>If space exists for agents.</returns>
    public bool CheckForSpace() => (!usingAgentLimits || occupyingAgents.Count < agentLimitCount);
}
