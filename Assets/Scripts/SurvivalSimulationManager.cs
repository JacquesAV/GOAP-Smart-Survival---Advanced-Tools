using UnityEngine;

/// <summary>
/// The main manager for the simulation, intended to manage most simulation features.
/// </summary>
public class SurvivalSimulationManager : MonoBehaviour
{
    [Header("Agent Settings")]
    /// <summary>
    /// The base chance for an agent to survive a given day.
    /// </summary>
    [Range(0, 100)]
    public float baseSurvivalChance = 50;

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
    /// Debugs the amount of food gathered by the agent.
    /// </summary>
    /// <param name="foodCount">The amount of food delivered.</param>
    public void DebugFood(int foodCount) => Debug.Log(string.Concat("Food: ", foodCount));

    /// <summary>
    /// Initialize core functionalities of the simulation.
    /// </summary>
    public void Start()
    {
        foodGenerator.GenerateFoodInArea();
    }

    /// <summary>
    /// Listen for keyboard input in temporary method for changing night state.
    /// </summary>
    public void Update()
    {
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
