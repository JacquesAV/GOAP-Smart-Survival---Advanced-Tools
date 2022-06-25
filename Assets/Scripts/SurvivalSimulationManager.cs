using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// The main manager for the simulation, intended to manage most simulation features.
/// </summary>
public class SurvivalSimulationManager : MonoBehaviour
{
    [Header("Survivor Settings")]
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
    /// The penalty to an agents survival if they are caught at the end of the day without reaching a home point.
    /// </summary>
    [Range(0, 100)]
    public float homelessPenalty = 25;

    /// <summary>
    /// The penalty to an agents survival if they are caught hungry at the end of the day.
    /// </summary>
    [Range(0, 100)]
    public float hungerPenalty = 25;

    /// <summary>
    /// Declaration with automatic getters and setters for the active survival manager.
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
    /// Stopwatch for the current simulation.
    /// </summary>
    [SerializeField]
    private CustomTimer simulationTimer;

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

    [Header("Practical Simulation Data")]
    /// <summary>
    /// The name and path of the current simulation, important in folder sorting.
    /// </summary>
    [SerializeField]
    private string simulationPathName;

    /// <summary>
    /// The number of the current generation being simulated.
    /// </summary>
    [SerializeField]
    [Range(1, 500)]
    private int generationNumber = 0;

    /// <summary>
    /// The maximum number before the simulation should automatically stop.
    /// </summary>
    [SerializeField]
    private int generationLimit = 100;

    [Header("Technical Simulation Data")]
    /// <summary>
    /// Track if the simulation is currently running or not
    /// </summary>
    [SerializeField]
    private bool simulationRunning = false;

    /// <summary>
    /// Declares how long each generation should run for.
    /// </summary>
    [SerializeField]
    private float simulationDuration = 20;

    /// <summary>
    /// Declares how much unity must speed up the simulation.
    /// </summary>
    [SerializeField]
    [Range(1, 10)]
    private float simulationTimeScale = 1;

    /// <summary>
    /// The ratio for how much of the simulation is considered daytime.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    private float dayRatio = 0.5f;

    /// <summary>
    /// The path for the simulations generations.
    /// </summary>
    private string primaryGenerationPath;

    /// <summary>
    /// The path for the surviving agents.
    /// </summary>
    private string primarySurvivedPath;

    /// <summary>
    /// The path for the dead agents.
    /// </summary>
    private string primaryDiedPath;

    /// <summary>
    /// Begin the simulation at the start of play.
    /// </summary>
    private void Start() => StartSimulation();

    /// <summary>
    /// Track possible changes in the world and update.
    /// </summary>
    private void Update()
    {
        // Return early if simulation is not running.
        if (!simulationRunning)
            return;

        // Ensure that the world is set as intended.
        ValidateWorldElements();

        // Run the end of simulation if the timer runs out.
        if(!simulationTimer.TimerIsRunning)
        {
            // Share food if applicable.
            GenerosityFoodShareRoll();

            // Roll for agents survival and save them accordingly.
            bool hasSurvivors = SurvivalCheckAndSaving();

            // Check if the simulation should stop and return early if so.
            if(SimulationStopCheck() || !hasSurvivors)
            {
                StopSimulation();
                return;
            }

            // Run the next generation with new data.
            NextGeneration();
        }
    }

    /// <summary>
    /// Ensures that certain essential world elements are being correctly tracked.
    /// </summary>
    private void ValidateWorldElements()
    {
        // Change if the world is considered to be night based on the current time.
        if (simulationTimer.TimeRemaining <= (simulationDuration * (1 - dayRatio)))
        {
            GWorld.Instance.GetWorld().SetState("IsNight", 1);
        }
        else
        {
            GWorld.Instance.GetWorld().RemoveState("IsNight");
        }

        // Clean the world of null references.
        GWorld.Instance.RemoveNullReferences();

        // While food points are believed to exist, check if they do.
        GWorld.Instance.ValidateWorldObjectState("FoodPointExists", GWorld.foodPoints);

        // While home points are believed to exist, check if they do.
        GWorld.Instance.ValidateWorldObjectState("ReturnedHome", GWorld.homePoints);
    }

    /// <summary>
    /// Checks if the simulation should stop based on specific conditions.
    /// </summary>
    private bool SimulationStopCheck() => (generationNumber >= generationLimit) ? true : false;

    /// <summary>
    /// End the simulation before the next generation.
    /// </summary>
    public void StopSimulation()
    {
        simulationRunning = false;
        simulationTimer.StopTimer();
        
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #endif
    }

    /// <summary>
    /// Start the simulation for the next generation.
    /// </summary>
    public void StartSimulation()
    {
        // Set the timescale for the simulation.
        Time.timeScale = simulationTimeScale;

        // Begin main simulation settings.
        simulationRunning = true;
        simulationTimer.StartTimer(simulationDuration);

        // Build the asset path.
        BuildDirectoryPaths();

        // Clean the simulation of possibly older data.
        CleanSimulation();

        // Start the first generation.
        RunNewGeneration();
    }

    /// <summary>
    /// Runs a generation of the survival simulation, increasing the current generation number.
    /// </summary>
    private void NextGeneration()
    {
        generationNumber++;
        StartSimulation();
    }

    /// <summary>
    /// Cleans the simulation of potentially older content.
    /// </summary>
    private void CleanSimulation()
    {
        // Destroys old agents from previous simulation.
        for (int i = activeAgents.Count - 1; i >= 0; i--)
        {
            Destroy(activeAgents[i]);
        }

        // Clear the current list of agents in case of carry over.
        activeAgents.Clear();

        // Destroy old food that should not exist in the current run.
        foodGenerator.RemoveFoodGlobal();

        // Unreserve home points of old agents.
        homeActionPoints.ForEach(x => x.UnreserveAllActionPoint());

        // Reset the world of its data.
        GWorld.Instance.ResetWorld();
    }

    /// <summary>
    /// Runs a generation of the survival simulation.
    /// </summary>
    private void RunNewGeneration()
    {
        // Return early and debug an error if there are more agents than the generation size.
        if (parentAgentPrefabs.Count > generationSize)
        {
            Debug.LogError("Too many parent prefabs for the intended generation size!");
            return;
        }

        // Generate food for the simulation.
        foodGenerator.GenerateFoodInArea();

        // List consisting of base agent prefabs ready for simulation instantiation.
        List<GameObject> baseAgentList = new List<GameObject>();
        baseAgentList.Populate(baseAgent, generationSize);

        // Create a queue for them.
        Queue<GameObject> queuedAgents = baseAgentList.ToQueue();

        // Creates an array of a list of objects that will be assosiated with a homepoint.
        List<GameObject>[] homePointAgents = new List<GameObject>[homeActionPoints.Count];
        for (int i = 0; i < homePointAgents.Length; i++)
        {
            homePointAgents[i] = new List<GameObject>();
        }

        // While the queued agents is full, distribute the agents evenly across the home points.
        while (queuedAgents.Count > 0)
        {
            foreach (List<GameObject> agents in homePointAgents)
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
        Queue<SurvivalGenes> previousGenes = parentAgents.ToQueue();

        // Awaken and initialize all agents for the simulation.
        foreach (GameObject agent in activeAgents)
        {
            if (agent.TryGetComponent(out SurvivalAgent survivalist))
            {
                // Apply a parent gene and mutate the agent.
                survivalist.SetBaseGenes(previousGenes.Dequeue());
                survivalist.MutateGenes(mutationRange);
                survivalist.generationNumber = generationNumber;
                survivalist.isAsleep = false;
            }
            else
            {
                Debug.LogWarning("No SurvivalAgent detected on agent GameObject!");
            }
        }
    }

    /// <summary>
    /// Runs logic cycle for an agent to share food with a starving agent.
    /// </summary>
    private void GenerosityFoodShareRoll()
    {
        // Temporary list for agents that are starving and will be willing to share.
        List<SurvivalAgent> starvingAgents = new List<SurvivalAgent>();
        List<SurvivalAgent> generousAgents = new List<SurvivalAgent>();
        foreach (GameObject agent in activeAgents)
        {
            if (agent.TryGetComponent(out SurvivalAgent survivalist))
            {
                // If starving, add to the starving agents list.
                if (survivalist.IsStarving())
                {
                    starvingAgents.Add(survivalist);
                }
                // Otherwise check if they are able and willing to share food.
                else if (survivalist.CanBeGenerous() && survivalist.RollForGenerosity())
                {
                    generousAgents.Add(survivalist);
                }
            }
            else
            {
                Debug.LogWarning("No SurvivalAgent detected on agent GameObject!");
            }
        }

        // Shuffle both lists for non-biased distribution.
        starvingAgents.Shuffle();
        generousAgents.Shuffle();

        // Convert the generous agents into a queue for ease of usage.
        Queue<SurvivalAgent> generousAgentsQueue = generousAgents.ToQueue();

        // Allow each generous agent to gift one piece of food to a starving agent (if any).
        foreach (SurvivalAgent agent in starvingAgents)
        {
            // If a generous agent remains, make an exhchange.
            if (generousAgentsQueue.Count > 0)
            {
                // Exchange food between the agents.
                SurvivalAgent gifter = generousAgentsQueue.Dequeue();
                gifter.wasGenerous = true;
                gifter.inventory.UnsafeRemoveFood(1);
                agent.inventory.AddFood(1);
                agent.beliefs.ModifyState("HasFood", 1);
            }
        }
    }

    /// <summary>
    /// Runs logic cycle for if an agent survives or dies and saves the relevant prefabs.
    /// </summary>
    /// <returns>If there are agents that survived.</returns>
    private bool SurvivalCheckAndSaving()
    {
        // Temporary list for agents that would survive.
        List<GameObject> survivingPrefabs = new List<GameObject>();
        int survivorCount = 0;
        int deceasedCount = 0;
        foreach (GameObject agent in activeAgents)
        {
            if (agent.TryGetComponent(out SurvivalAgent survivalist))
            {
                // Recalculate survival chances in case they are not updated.
                survivalist.UpdateSurvivalChance();

                // Save appropriately based on survival roll.
                if(survivalist.RollForSurvival())
                {
                    survivorCount++;
                    survivalist.survivalState = SurvivalState.SURVIVED;
                    survivingPrefabs.Add(PrefabUtility.SaveAsPrefabAsset(agent, string.Concat(primarySurvivedPath, "/Survivor", survivorCount, ".prefab")));
                }
                else
                {
                    deceasedCount++;
                    survivalist.survivalState = SurvivalState.DIED;
                    PrefabUtility.SaveAsPrefabAsset(agent, string.Concat(primaryDiedPath, "/Deceased", deceasedCount, ".prefab"));
                }
            }
            else
            {
                Debug.LogWarning("No SurvivalAgent detected on agent GameObject!");
            }
        }

        // Stop the simulation if no agents survived.
        if(survivingPrefabs.Count == 0)
        {
            Debug.Log("All agents died, in the simulation!");
            return false;
        }
        else
        {
            parentAgentPrefabs = survivingPrefabs;
            return true;
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
    /// <param name="parentAgents">The list of survival agent parents being used in generation.</param>
    /// <param name="generationCount">The size of the generation.</param>
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

    /// <summary>
    /// Builds folder paths specific to the current generation.
    /// </summary>
    private void BuildDirectoryPaths()
    {
        primaryGenerationPath = Path.Combine("Assets", simulationPathName, "Gen-" + generationNumber);
        BuildDirectoryPath(primaryGenerationPath);

        primarySurvivedPath = Path.Combine(primaryGenerationPath, "Survived");
        BuildDirectoryPath(primarySurvivedPath);

        primaryDiedPath = Path.Combine(primaryGenerationPath, "Died");
        BuildDirectoryPath(primaryDiedPath);
    }

    /// <summary>
    /// Builds a given path in the directory.
    /// </summary>
    /// <param name="assetPath">The given path to create the directory at.</param>
    private void BuildDirectoryPath(string assetPath)
    {
        if (Directory.Exists(assetPath))
        {
            //Debug.Log(string.Concat("Path already exists: ", assetPath));
            return;
        }

        Directory.CreateDirectory(assetPath);
        AssetDatabase.Refresh();
    }
}
