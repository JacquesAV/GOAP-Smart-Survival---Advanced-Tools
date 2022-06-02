using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Planner class that allows for any <see cref="GAgent"/> to be able to create valid action plans.
/// </summary>
public class GPlanner
{
    /// <summary>
    /// Boolean to mark if debugging should happen.
    /// </summary>
    public bool shouldDebug = true;

    /// <summary>
    /// Constructor for quick creation of the class.
    /// </summary>
    /// <param name="shouldDebug">If the planner should debug or not.</param>
    public GPlanner(bool shouldDebug) => this.shouldDebug = shouldDebug;

    /// <summary>
    /// Construct a plan of actions based on given parameters..
    /// </summary>
    /// <param name="actions">List of available actions that the agent can take.</param>
    /// <param name="goal">List of goals that the agent will seek to accomplish.</param>
    /// <param name="beliefStates">List of beliefs that the agent has about itself and the world.</param>
    /// <returns>A queue of actions that the agent can follow.</returns>
    public Queue<GAction> PlanActions(List<GAction> actions, Dictionary<string, int> goal, WorldStates beliefStates)
    {
        // Create a list for the usable actions.
        List<GAction> usableActions = new List<GAction>();

        // If an action is achievable, then this will be added to the usable actions.
        // This clears the list of unusable actions that would otherwise clutter the list.
        foreach (GAction a in actions) 
        {
            if (a.IsAchievable())
            {
                usableActions.Add(a);
            }
        }

        // List to track the created branches.
        List<Node> branches = new List<Node>();

        // Create the first branch of the node graph that will be created.
        Node start = new Node(null, 0.0f, GWorld.Instance.GetWorld().States, beliefStates.States, null);

        // See if a graph is buildable (path for the actions).
        bool success = BuildGraph(start, branches, usableActions, goal);

        // If not possible, fail.
        if (!success)
        {
            DebugLocal("NO PLAN");
            return null;
        }
        
        // Begin finding the cheapest nodes to follow.
        Node cheapest = null;

        // Loop over each, looking for the cheapest leaf to follow.
        foreach (Node branch in branches)
        {
            // If nothing was selected as the cheapest yet, then use the branch as the first cheapest option.
            if (cheapest == null)
            {
                cheapest = branch;
            }
            // If cheaper than the previous branch, use.
            else if (branch.cost < cheapest.cost) 
            {
                cheapest = branch;
            }
        }

        // Loop back around.
        // List of actions that are the result.
        List<GAction> result = new List<GAction>();
        Node node = cheapest;

        // While the node and actions are not null.
        while (node != null)
        {
            if (node.action != null)
            {
                // Place action at start of the list.
                result.Insert(0, node.action);
            }

            // Set parent of the node.
            node = node.parent;
        }

        // Queue of the actions to take.
        Queue<GAction> queue = new Queue<GAction>();

        // Queue up new actions.
        foreach (GAction a in result)
        {
            queue.Enqueue(a);
        }

        // Debug the current plan that the NPC will take.
        DebugLocal("The Plan is: ");
        foreach (GAction a in queue)
        {
            DebugLocal("Q: " + a.actionName);
        }

        return queue;
    }

    /// <summary>
    /// Build a node graph of the usable actions for an agent.
    /// </summary>
    /// <param name="parent">The previous action that the agent planned to take.</param>
    /// <param name="branches">The current path of actions being explored.</param>
    /// <param name="usableActions">List of actions that the agent could do.</param>
    /// <param name="goal">Dictionary of goals that the agent is seeking to complete.</param>
    /// <returns>If a valid action plan was created. </returns>
    private bool BuildGraph(Node parent, List<Node> branches, List<GAction> usableActions, Dictionary<string, int> goal) {

        // Bool to track wether or not a valid action path has been found.
        bool foundActionPath = false;

        // Iterate over each action that can currently be executed.
        foreach (GAction action in usableActions)
        {
            // If the action can be achieved.
            if (action.IsAchievableGiven(parent.state))
            {
                // Declare dictionary of the current state based on the parent.
                Dictionary<string, int> currentState = new Dictionary<string, int>(parent.state);

                // Iterate over the action effects that are contained by the current states.
                foreach (KeyValuePair<string, int> eff in action.aftereffects)
                {
                    // If the current state does not already contain it, add it.
                    if (!currentState.ContainsKey(eff.Key))
                    {
                        currentState.Add(eff.Key, eff.Value);
                    }
                }

                // Create a new node based on the previous results.
                Node node = new Node(parent, parent.cost + action.cost, currentState, action);

                // If the goal is achieved, then add as a valid branch and declare that the path was found.
                if (GoalAchieved(goal, currentState))
                {
                    branches.Add(node);
                    foundActionPath = true;

                    DebugLocal("Path was found");
                }
                // If no path was found, move onto the next node.
                else
                {
                    // Create a subset for the action to take out the current action in the node.
                    // This will allow for future graphs to be built without creating ubreakable loops or unnecesary repetition.
                    List<GAction> subset = ActionSubset(usableActions, action);

                    // Build a new graph with this new subset.
                    // Recursive call.
                    bool found = BuildGraph(node, branches, subset, goal);

                    // Set the path as found if one is discovered.
                    if (found)
                    {
                        foundActionPath = true;
                    }
                }
            }
        }
        // Return that the path was found.
        return foundActionPath;
    }

    /// <summary>
    /// Removes a given action from an action list path.
    /// </summary>
    /// <param name="actions">The list of actions being modified.</param>
    /// <param name="actionToRemove">The action being removed from the list.</param>
    /// <returns>The new list of actions cleared of the old action.</returns>
    private List<GAction> ActionSubset(List<GAction> actions, GAction actionToRemove)
    {
        // Declare list of actions to return.
        List<GAction> subset = new List<GAction>();

        // Iterate over the list of actions.
        foreach (GAction a in actions)
        {
            // Remove the action from the subset list.
            if (!a.Equals(actionToRemove))
            {
                subset.Add(a);
            }
        }

        // Return the subset.
        return subset;
    }

    /// <summary>
    /// Check for if any given goal is achieved based on current states.
    /// </summary>
    /// <param name="goal">The goals being checked.</param>
    /// <param name="state">The current states of the agent.</param>
    /// <returns>If the given goals of the agent were met.</returns>
    private bool GoalAchieved(Dictionary<string, int> goal, Dictionary<string, int> state)
    {
        // Iterate over each pair of goals.
        foreach (KeyValuePair<string, int> g in goal)
        {
            // If the state does not contain the goal, return false.
            if (!state.ContainsKey(g.Key))
            {
                return false;
            }
        }
        // Otherwise return true.
        return true;
    }

    /// <summary>
    /// Debug a given string if debugging is enabled.
    /// </summary>
    /// <param name="givenString">The string to debug.</param>
    private void DebugLocal(string givenString)
    {
        if(shouldDebug)
        {
            Debug.Log(givenString);
        }
    }
}
