using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The main manager for the simulation, intended to manage most simulation features.
/// </summary>
public class SurvivalSimulationManager : MonoBehaviour
{
    [Header("Survival Settings")]
    /// <summary>
    /// The range at which an agents gene can mutate.
    /// </summary>
    [Range(0, 1)]
    public float mutationRange = 0.1f;

    /// <summary>
    /// The base chance for an agent to survive a given day.
    /// </summary>
    [Range(0, 100)]
    public float baseSurvivalChance = 50;

    /// <summary>
    /// How much food affects the survival chance.
    /// </summary>
    [Range(0, 100)]
    public float foodSurvivalBoost = 25;

    /// <summary>
    /// How much hardiness affects the survival chance.
    /// </summary>
    [Range(0, 100)]
    public float hardinessSurvivalBoost = 25;

    /// <summary>
    /// How much speed is boosted by based on the hardiness speed curve.
    /// </summary>
    [Range(0, 10)]
    public float speedInfluenceChange = 5;

    /// <summary>
    /// The maximum amount of food when considering food survival chance calculations.
    /// </summary>
    [Range(0, 10)]
    public int foodCapacity = 5;

    /// <summary>
    /// Array for the data of all surviving agents in the last simulation cycle.
    /// </summary>
    [SerializeField]
    private SurvivalGenes[] lastSurvivalAgents;

    [Header("Survival Curves")]
    /// <summary>
    /// The falloff curve representing the influence levels of food.
    /// </summary>
    public AnimationCurve foodCurve;

    /// <summary>
    /// The curve representing the relationship between hardiness and speed.
    /// </summary>
    public AnimationCurve hardinessSpeedCurve;

    [Header("Enviornmental Survival Penalties")]
    /// <summary>
    /// The penalty to an agents survival if they are caught in the dark at the end of the day
    /// </summary>
    [Range(0, 100)]
    public float darknessPenalty = 25;

    /// <summary>
    /// The penalty to an agents survival if they are caught hungry at the end of the day.
    /// </summary>
    [Range(0, 100)]
    public float hungerPenalty = 25;

    /// <summary>
    /// Declaration with automatic getters and setters for the active survival manager
    /// </summary>
    public static SurvivalSimulationManager SingletonManager { get; private set; }

    [Header("Repopulation Settings")]
    /// <summary>
    /// The percentage of agent repopulation that is reserved exclusively for generous agents.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    private float generousRepopulationRatio= 0.25f;

    /// <summary>
    /// Set the singleton to this instance.
    /// </summary>
    private void Awake() => SingletonManager = this;

    [Header("Simulation Components")]
    /// <summary>
    /// The generator object for food in the simulation.
    /// </summary>
    [SerializeField]
    private FoodGenerator foodGenerator;

    /// <summary>
    /// The generator object for food in the simulation.
    /// </summary>
    [SerializeField]
    private List<HomeActionPoint> homeActionPoints;

    /// <summary>
    /// List of agent prefabs entered as parents to be used in the simulation.
    /// </summary>
    [SerializeField]
    private List<GameObject> parentAgentPrefabs;

    /// <summary>
    /// List of active agents in the simulation.
    /// </summary>
    private List<GameObject> activeAgents = new List<GameObject>();

    /// <summary>
    /// The base template for survival agents.
    /// </summary>
    public GameObject baseAgent;

    /// <summary>
    /// How big each generation should be.
    /// </summary>
    [Range(1, 50)]
    public int generationSize;

    /// <summary>
    /// Initialize core functionalities of the simulation.
    /// </summary>
    public void Start()
    {
        // Return early and debug an error if there are more agents than the generation size.
        if(parentAgentPrefabs.Count > generationSize)
        {
            Debug.LogError("Too many parent prefabs for the intended generation size!");
            return;
        }

        // Clear the current list of agents in case of carry over.
        activeAgents.Clear();

        // Generate food for the simulation.
        foodGenerator.RemoveFood();
        foodGenerator.GenerateFoodInArea();

        // List consisting of base agent prefabs ready for simulation instantiation.
        List<GameObject> baseAgentList = new List<GameObject>();
        baseAgentList.Populate(baseAgent, generationSize);

        // Create a queue for them.
        Queue<GameObject> queuedAgents = baseAgentList.ConvertListToQueue();

        List<GameObject>[] homePointAgents = new List<GameObject>[homeActionPoints.Count];
        for (int i = 0; i < homePointAgents.Length; i++)
        {
            homePointAgents[i] = new List<GameObject>();
        }

        // While the queued agents is full, distribute the agents evenly across the home points.
        while (queuedAgents.Count > 0)
        {
            foreach(List<GameObject> agents in homePointAgents)
            {
                if (queuedAgents.Count > 0)
                {
                    // Dequeue an agent into the list.
                    agents.Add(queuedAgents.Dequeue());
                }
                else
                {
                    // Break the distribution early if empty.
                    break;
                }
            }
        }

        // Pass in the distributed list of agents.
        for (int i = 0; i < homeActionPoints.Count; i++)
        {
            activeAgents.AddRange(homeActionPoints[i].GenerateAgentsInArea(homePointAgents[i]));
        }

        // Using the previous surviving simulation agents, generate a gene pool based on their results.
        List<SurvivalGenes> parentAgents = GenerateGenePool(parentAgentPrefabs.GetComponentFromList<SurvivalAgent>(), generationSize);
        parentAgents.Shuffle();
        Queue<SurvivalGenes> previousGenes = parentAgents.ConvertListToQueue();

        // Awaken and initialize all agents for the simulation.
        foreach (GameObject agent in activeAgents)
        {
            if (agent.TryGetComponent(out SurvivalAgent survivalist))
            {
                // Apply a parent gene and mutate the agent.
                survivalist.SetBaseGenes(previousGenes.Dequeue());
                survivalist.MutateGenes(mutationRange);
                survivalist.isAsleep = false;
            }
            else
            {
                Debug.LogWarning("No SurvivalAgent detected on agent GameObject!");
            }
        }
    }

    /// <summary>
    /// Track possible changes in the world and update.
    /// </summary>
    public void Update()
    {
        // While food points are believed to exist, check if they do.
        GWorld.Instance.ValidateWorldObjectState("FoodPointExists", GWorld.foodPoints);

        // While home points are believed to exist, check if they do.
        GWorld.Instance.ValidateWorldObjectState("ReturnedHome", GWorld.homePoints);

        // Temporary check for inputs to toggle night.
        if (Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            if (GWorld.Instance.GetWorld().HasState("IsNight"))
            {
                GWorld.Instance.GetWorld().RemoveState("IsNight");
                Debug.Log("Set to day");
            }
            else
            {
                GWorld.Instance.GetWorld().SetState("IsNight", 1);
                Debug.Log("Set to night");
            }
        }
    }

    /// <summary>
    /// Disables agents that returned home in order to prevent collision issues or accidental overlaps.
    /// </summary>
    public void AgentReturnedHome(SurvivalAgent returnedAgent)
    {
        // Sleep the agent and disable its collisions.
        returnedAgent.isAsleep = true;
        returnedAgent.gameObject.GetComponent<CircleCollider2D>().enabled = false;
    }

    /// <summary>
    /// Generates a list of survival genes based on a list of given parents and the intended size for the new gene pool
    /// </summary>
    /// <param name="parentAgents"></param>
    /// <param name="generationCount"></param>
    /// <returns>A list of survival genes.</returns>
    private List<SurvivalGenes> GenerateGenePool(List<SurvivalAgent> parentAgents, int generationCount)
    {
        // Temporary list to hold survival genes generated from the pool of parents.
        List<SurvivalGenes> genePool = new List<SurvivalGenes>();

        // Return with default genes if no parents were given.
        if (parentAgents.Count == 0)
        {
            Debug.Log("No parent agents provided, generating pool with default survival genes!");
            genePool.Populate(new SurvivalGenes(), generationCount);
            return genePool;
        }

        // Temporary list of parent agents that were generous.
        List<SurvivalAgent> generousAgents = parentAgents.FindAll(x => x.wasGenerous);

        // Add one guaranteed instance of the current parents to the gene pool.
        genePool.AddRange(from item in parentAgents select item.CopiedGenes);

        // Calculate how many additional agents need to be generated.
        float outstandingAgents = generationCount - parentAgents.Count;

        // Calculate how many additional agents are reserved for generous agents, if any agents were generous.
        float generousReservations = outstandingAgents * (generousAgents.Count > 0 ? generousRepopulationRatio : 0);

        // Appropriately reduce and round the resulting values.
        outstandingAgents = Mathf.CeilToInt(outstandingAgents - generousReservations);
        generousReservations = Mathf.FloorToInt(generousReservations);

        // Add to the gene pool while space still remains.
        while ((int)generousReservations > 0)
        {
            // Get a random gene from the parent agents and reduce the count.
            genePool.Add(generousAgents.GetRandom().CopiedGenes);
            generousReservations--;
        }
        while ((int)outstandingAgents > 0)
        {
            // Get a random gene from the parent agents and reduce the count.
            genePool.Add(parentAgents.GetRandom().CopiedGenes);
            outstandingAgents--;
        }
        return genePool;
    }
}
