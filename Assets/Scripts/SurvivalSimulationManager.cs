using System.Collections.Generic;
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
    public float mutationRange = 0.15f;

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
    /// List of agent prefabs to be used in the simulation.
    /// </summary>
    private List<GameObject> agentPrefabs;

    /// <summary>
    /// List of active agents in the simulation.
    /// </summary>
    private List<GameObject> activeAgents = new List<GameObject>();

    /// <summary>
    /// Temporary agent prefab for testing.
    /// </summary>
    public GameObject temporaryAgent;

    /// <summary>
    /// Temporary int for agent spawning.
    /// </summary>
    public int temporaryAgentsPerArea;

    /// <summary>
    /// Debugs the amount of food gathered by the agent.
    /// </summary>
    /// <param name="foodCount">The amount of food delivered.</param>
    public void DebugFood(int foodCount) => Debug.Log(string.Concat("Food: ", foodCount));

    /// <summary>
    /// Initialize core functionalities of the simulation.
    /// </summary>
    public void Start()
    {
        // Clear the current list of agents in case of carry over.
        activeAgents.Clear();

        // Generate food for the simulation.
        foodGenerator.RemoveFood();
        foodGenerator.GenerateFoodInArea();

        // Temporary new list for testing.
        List<GameObject> testingAgentList = new List<GameObject>();
        testingAgentList.Populate(temporaryAgent, temporaryAgentsPerArea);

        // For now mutate and sleep simply from here.
        foreach (GameObject agent in testingAgentList)
        {
            if(agent.TryGetComponent(out SurvivalAgent survivalist))
            {
                survivalist.MutateGenes(mutationRange);
                survivalist.isAsleep = true;
            }
            else
            {
                Debug.LogWarning("No SurvivalAgent detected on agent GameObject!");
            }
        }

        // For now use just one prefab repeatedly.
        foreach (HomeActionPoint home in homeActionPoints)
        {
            activeAgents.AddRange(home.GenerateAgentsInArea(testingAgentList));
        }

        // Awaken all agents for the simulation.
        foreach (GameObject agent in activeAgents)
        {
            if (agent.TryGetComponent(out SurvivalAgent survivalist))
            {
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
}
