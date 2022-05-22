using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Defines the data that makes up a given type of action.
/// </summary>
public abstract class GAction : MonoBehaviour
{
    /// <summary>
    /// Name of the action.
    /// </summary>
    public string actionName = "Action";

    /// <summary>
    /// Cost & favorability of the particular action.
    /// </summary>
    public float cost = 1.0f;

    /// <summary>
    /// Current target object for interaction.
    /// </summary>
    public GameObject target;

    /// <summary>
    /// Current target object for interaction.
    /// </summary>
    public ActionPoint targetActionPoint;

    /// <summary>
    /// Tag for the target.
    /// </summary>
    public string targetTag;

    /// <summary>
    /// Duration of the action.
    /// </summary>
    public float duration = 0;

    /// <summary>
    /// Set of conditions that prevents action from being possible.
    /// </summary>
    public WorldState[] antiConditions;

    /// <summary>
    /// Action conditions that must be met before an action can take place.
    /// </summary>
    public WorldState[] preConditions;

    /// <summary>
    /// The after effects of completing an action.
    /// </summary>
    public WorldState[] afterEffects;

    /// <summary>
    /// The agent that will move.
    /// </summary>
    [HideInInspector] public NavMeshAgent navAgent;

    /// <summary>
    /// The GOAP attached agent.
    /// </summary>
    [HideInInspector] public GAgent gAgent;

    /// <summary>
    /// Dictionary of anticonditions.
    /// </summary>
    public Dictionary<string, int> anticonditions;

    /// <summary>
    /// Dictionary of preconditions.
    /// </summary>
    public Dictionary<string, int> preconditions;

    /// <summary>
    /// Dictionary of effects.
    /// </summary>
    public Dictionary<string, int> aftereffects;

    /// <summary>
    /// State of the agent itself (internal set of states).
    /// </summary>
    [HideInInspector] public WorldStates agentBeliefs;

    /// <summary>
    /// If the state is currently running.
    /// </summary>
    public bool running = false;

    /// <summary>
    /// Constructor for quick creation of action.
    /// </summary>
    public GAction()
    {
        // Create new dictionaries.
        anticonditions = new Dictionary<string, int>();
        preconditions = new Dictionary<string, int>();
        aftereffects = new Dictionary<string, int>();
    }

    /// <summary>
    /// Set up the action for usage by an agent.
    /// </summary>
    public void Awake()
    {
        // Get the navmesh agent.
        navAgent = gameObject.GetComponent<NavMeshAgent>();
        gAgent = GetComponent<GAgent>();
        agentBeliefs = GetComponent<GAgent>().beliefs;

        // If anticonditions exists, add from world states.
        if (antiConditions != null)
        {
            foreach (WorldState condition in antiConditions)
            {
                anticonditions.Add(condition.key, condition.value);
            }
        }

        // If preconditions exists, add from world states.
        if (preConditions != null)
        {
            foreach (WorldState condition in preConditions)
            {
                preconditions.Add(condition.key, condition.value);
            }
        }

        // If after effect exists, add from world states.
        if (afterEffects != null)
        {
            foreach (WorldState effect in afterEffects)
            {
                aftereffects.Add(effect.key, effect.value);
            }
        }
    }

    /// <summary>
    /// If action can be completed.
    /// </summary>
    /// <returns>If the action can be achieved.</returns>
    public bool IsAchievable() => PrePerform();

    /// <summary>
    /// If action can be completed based on set of pre-conditions or set of anti-conditions.
    /// </summary>
    /// <param name="conditions">List of conditions to consider.</param>
    /// <returns></returns>
    public bool IsAchievableGiven(Dictionary<string, int> conditions)
    {
        // Compare against preconditions.
        foreach (KeyValuePair<string, int> pc in preconditions)
        {
            if (!conditions.ContainsKey(pc.Key))
            {
                return false;
            }
        }

        // Compare against anti-conditions.
        foreach (KeyValuePair<string, int> ac in anticonditions)
        {
            if (conditions.ContainsKey(ac.Key))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Abstract method for pre performing an action.
    /// </summary>
    /// <returns>If sucessfully performed.</returns>
    public abstract bool PrePerform();

    /// <summary>
    /// Abstract method for intra performing an action.
    /// </summary>
    /// <returns>If sucessfully performed.</returns>
    public abstract bool IntraPerform();

    /// <summary>
    /// Abstract method for post performing an action.
    /// </summary>
    /// <returns>If sucessfully performed.</returns>
    public abstract bool PostPerform();

    /// <summary>
    /// Get the closest target for interaction.
    /// </summary>
    /// <param name="targets">List of potential targets.</param>
    /// <returns>The closest target.</returns>
    public GameObject GetClosestTarget(List<GameObject> targets)
    {
        // Track and look for the closest target.
        GameObject closestObject = null;
        float closestDistance = Mathf.Infinity;
        NavMeshPath path = new NavMeshPath();
        foreach (GameObject target in targets)
        {
            // Continue if null and throw a warning.
            if(!target)
            {
                Debug.LogWarning("Null object was skipped while searching for closest target!");
                continue;
            }

            // Sampled positions.
            NavMesh.SamplePosition(this.transform.position, out NavMeshHit originHit, gAgent.goalDistanceSentitivity / 2, NavMesh.AllAreas);
            NavMesh.SamplePosition(target.transform.position, out NavMeshHit destinationHit, gAgent.goalDistanceSentitivity / 2, NavMesh.AllAreas);

            // Calculate a path and continue if valid.
            if (originHit.hit && destinationHit.hit && NavMesh.CalculatePath(originHit.position, destinationHit.position, NavMesh.AllAreas, path))
            {
                // Continue if path is complete.
                if(path.status == NavMeshPathStatus.PathComplete)
                {
                    // Calculate distance.
                    float distance = ExtensionMethods.GetPathDistance(path.corners);

                    // Check if the distance is less that the current closest object.
                    if (closestDistance > distance)
                    {
                        closestObject = target;
                        closestDistance = distance;
                    }
                }
            }
        }

        return closestObject;
    }

    /// <summary>
    /// Get the closest target with an available action point for interaction.
    /// </summary>
    /// <param name="targets">List of potential targets.</param>
    /// <returns>The closest target with an available action point.</returns>
    public GameObject GetClosestAvailableActionPointTarget(List<GameObject> targets)
    {
        // Temporary list of targets with available action points.
        List<GameObject> actionPointTargets = new List<GameObject>();

        // Populate the list with valid targets.
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            GameObject potentialTarget = targets[i];
            if(potentialTarget.TryGetComponent(out ActionPoint point) && point.CheckForSpace())
            {
                actionPointTargets.Add(potentialTarget);
            }
        }

        // Check for the closest valid target.
        return GetClosestTarget(targets);
    }

    public static bool GetPath(NavMeshPath path, Vector3 fromPos, Vector3 toPos, int passableMask)
    {
        path.ClearCorners();

        if (NavMesh.CalculatePath(fromPos, toPos, passableMask, path) == false)
            return false;

        return true;
    }

    /// <summary>
    /// Get if the path is possible.
    /// </summary>
    /// <returns>If the path if complete.</returns>
    public bool HasCompletePath()
    {
        if(navAgent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            return true;
        } 
        else
        {
            return false;
        }
    }
}
