using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(GInventory))]
public class GAgent : MonoBehaviour
{
    /// <summary>
    /// List of actions that the agent has access to.
    /// </summary>
    public List<GAction> actions = new List<GAction>();

    /// <summary>
    /// List of subgoals the agent will seek to achieve.
    /// </summary>
    public Dictionary<SubGoal, int> goals = new Dictionary<SubGoal, int>();

    /// <summary>
    /// List of agent beliefs about the world.
    /// </summary>
    public WorldStates beliefs = new WorldStates();

    /// <summary>
    /// Agents internal states.
    /// </summary>
    [HideInInspector] public GInventory inventory;

    /// <summary>
    /// Although unused in this class, this should be used for action classes to fetch.
    /// </summary>
    private NavMeshAgent navAgent;

    /// <summary>
    /// The planner that tracks actions.
    /// </summary>
    private GPlanner planner;

    /// <summary>
    /// Queue for the actions that will be taken.
    /// </summary>
    private Queue<GAction> actionQueue;

    /// <summary>
    /// The action that is currently being taken.
    /// </summary>
    public GAction currentAction;

    /// <summary>
    /// Current goal that the agent will seek to achieve.
    /// </summary>
    public SubGoal currentGoal;

    /// <summary>
    /// Distance for a goal to be valid.
    /// </summary>
    [Range(1.5f,2.5f)]
    public float goalDistanceSentitivity = 2f;

    /// <summary>
    /// Tracks is the current action has been invoked.
    /// </summary>
    private bool invoked = false;

    /// <summary>
    /// Quick bool return for if the agent is still calculating a path.
    /// </summary>
    /// <returns>If the path is still pending for calculation.</returns>
    public bool IsPathPending => navAgent.pathPending;

    /// <summary>
    /// Quick bool return for if the agent has a path.
    /// </summary>
    /// <returns>If the path is complete to the target.</returns>
    public bool HasPath => navAgent.hasPath;

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    public void Start()
    {
        // Clear agent of potentially old data.
        goals.Clear();
        actions.Clear();
        beliefs = new WorldStates();
        actionQueue.Clear();
        currentGoal = null;
        currentAction = null;
        planner = null;

        // Get the navmesh agent from the attached agent.
        navAgent = gameObject.GetComponent<NavMeshAgent>();

        // Disable forced rotation.
        navAgent.updateRotation = false;
        navAgent.updateUpAxis = false;

        // Create the array of actions that the agent has.
        GAction[] availableActions = this.GetComponents<GAction>();
        foreach (GAction action in availableActions)
        {
            actions.Add(action);
        }

        // Get the inventory if one is attached.
        if(gameObject.TryGetComponent(out GInventory inv))
        {
            inventory = inv;
        }
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    public void Update()
    {
        RunAgentLogic();
        CorrectSpriteOrientation();
    }

    /// <summary>
    /// Method that runs completion logic for the current action.
    /// </summary>
    public void CompleteAction()
    {
        // Finish running the action.
        currentAction.running = false;

        // Method that is run at the end of an action.
        currentAction.PostPerform();

        // Set invoked back to false, allowing for the next invoke test.
        invoked = false;

        // Reset the agents stored action.
        currentAction = null;

        // Reset the NavAgent.
        navAgent.ResetPath();
    }

    /// <summary>
    /// Core functionality of the GOAP agent.
    /// </summary>
    private void RunAgentLogic()
    {
        // If the current action is not currently executing.
        if (currentAction != null && currentAction.running)
        {
            // Check if the current action is still valid during transition and cancel if not.
            if (!currentAction.IntraPerform() && !invoked)
            {
                currentAction.targetActionPoint.UnreserveActionPoint(this);
                currentAction = null;
                return;
            }

            // Check that the agent has reached its goal
            if (Vector3.Distance(currentAction.target.transform.position, transform.position) <= goalDistanceSentitivity) //&& currentAction.agent.remainingDistance <= goalDistanceSentitivity)
            {
                // If not yet invoked, then attempt to perform it.
                if (!invoked)
                {
                    // Will complete the action after the actions duration.
                    Invoke(nameof(CompleteAction), currentAction.duration);
                    invoked = true;

                    // Reserve the action point if applicable.
                    if (currentAction.targetActionPoint && !currentAction.targetActionPoint.GlobalAllowance)
                    {
                        currentAction.targetActionPoint.ReserveActionPoint(this);
                    }
                }
            }
            return;
        }

        // If no plan or action queue exists, create one.
        if (planner == null || actionQueue == null)
        {
            planner = new GPlanner();

            // Sort the goals by descending value.
            var sortedGoals = from entry in goals orderby entry.Value descending select entry;

            // Loop through each goal and sort them.
            foreach (KeyValuePair<SubGoal, int> sg in sortedGoals)
            {
                // Set the action queue to what the planner can return based on the goal.
                actionQueue = planner.PlanActions(actions, sg.Key.sGoals, beliefs);

                // If the queue is not null then a plan exists.
                if (actionQueue != null)
                {
                    // Set the current goal.
                    currentGoal = sg.Key;

                    // Once the plan is found, break the loop.
                    break;
                }
            }
        }

        // If a queue exists and the action queue is empty.
        if (actionQueue != null && actionQueue.Count == 0)
        {
            // Check if the current goal can/should be removed.
            if (currentGoal.isRemovedOnCompletion)
            {
                // Remove the goal.
                goals.Remove(currentGoal);
            }
            // Nullify the planner in order to force the formation of a new plan.
            planner = null;
        }

        // If there are still actions to execute in the queue.
        if (actionQueue != null && actionQueue.Count > 0)
        {
            // Dequeue the top action of the queue and set it as the currentAction to execute.
            currentAction = actionQueue.Dequeue();

            // Check for pre-conditions.
            if (currentAction.PrePerform())
            {
                // If a target is not yet selected, find it.
                // Ideally this shouldnt happen and shouls be handles in the PrePerform.
                if (currentAction.target == null && currentAction.targetTag != "")
                {
                    currentAction.target = GameObject.FindWithTag(currentAction.targetTag);
                }

                // If the target is not null (valid target).
                if (currentAction.target != null)
                {
                    // Start running the current action.
                    currentAction.running = true;

                    // Set the destination for the navigation agent to move to.
                    currentAction.navAgent.SetDestination(currentAction.target.transform.position);
                }
            }
            else
            {
                // Force a new plan by nullifying the queue.
                actionQueue = null;
            }
        }
    }

    /// <summary>
    /// Orientates the sprite correctly.
    /// </summary>
    private void CorrectSpriteOrientation()
    {
        // Change based on agent velocity.
        if (navAgent.velocity.x < 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }
        else if (navAgent.velocity.x > 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
    }
}


