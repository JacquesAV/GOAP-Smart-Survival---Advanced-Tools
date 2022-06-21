using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// The agent that is dedicated to a survival simulation.
/// </summary>
public class SurvivalAgent : GAgent
{
    [Header("Agent Goals")]
    /// <summary>
    /// Priority level of returning home task, higher the number the more important.
    /// </summary>
    public IntMinMax returnHomePriority;

    /// <summary>
    /// Priority level of food collection task, higher the number the more important.
    /// </summary>
    public int foodCollectionPriority = 10;

    [Header("Agent Genes")]
    /// <summary>
    /// The base speed for the agent.
    /// </summary>
    [Range(0, 10)]
    public float baseSpeed = 2.5f;

    /// <summary>
    /// The active speed for the agents.
    /// </summary>
    private float finalSpeed;

    /// <summary>
    /// The natural hardiness of the agent, granting a higher base chance of survival over
    /// the natural speed of the agent, granting a higher movement speed.
    /// </summary>
    [Range(0, 1)]
    public float agentHardinessOverSpeed = 0.5f;

    /// <summary>
    /// The likelihood an agent will share excess food at the end of the day.
    /// </summary>
    [Range(0, 1)]
    public float agentGenerosity = 0.5f;

    /// <summary>
    /// Returns a copy of the the survival genes.
    /// </summary>
    public SurvivalGenes CopiedGenes => new SurvivalGenes(agentHardinessOverSpeed, agentGenerosity);

    [Header("Miscellaneous")]
    /// <summary>
    /// Boolean to track if the agent is considered dead or alive.
    /// </summary>
    public bool isAsleep = true;

    /// <summary>
    /// Boolean to track if the agent had gifted food.
    /// </summary>
    public bool wasGenerous = false;

    /// <summary>
    /// Boolean to track if the agent is considered dead or alive.
    /// </summary>
    public bool isDead = false;

    /// <summary>
    /// The current survival chance that a given agent has.
    /// </summary>
    private float survivalChance;

    [Header("UI")]
    /// <summary>
    /// Text to visually show the status of the agent.
    /// </summary>
    public TextMeshProUGUI statusText = null;

    /// <summary>
    /// Slider to visually show the survival chance of the agent.
    /// </summary>
    public Slider survivalBar = null;

    /// <summary>
    /// The collection goal for the agent.
    /// </summary>
    private SubGoal collectionGoal;

    /// <summary>
    /// The return home goal for the agent.
    /// </summary>
    private SubGoal returnedGoal;

    /// <summary>
    /// Start is called before the first frame update.
    /// Sets up the agent and all relevant parameters.
    /// </summary>
    new void Start()
    {
        base.Start();

        // Set starting survival chance.
        survivalChance = SurvivalSimulationManager.SingletonManager.baseSurvivalChance;

        // Set Maximum for survival bar.
        if (survivalBar != null) { survivalBar.maxValue = 100; }

        // Create & add goals with priorities, set to false to keep goal after completion.
        collectionGoal = new SubGoal("CollectedFood", 1, false);
        goals.Add(collectionGoal, foodCollectionPriority);

        returnedGoal = new SubGoal("ReturnedHome", 1, true);
        goals.Add(returnedGoal, returnHomePriority.integer);
    }

    /// <summary>
    /// Late update visual elements.
    /// </summary>
    new void LateUpdate()
    {
        // Return early if agent is asleep.
        if (isAsleep) return;

        // Update relevant priorities.
        UpdateReturnHomePriority();

        // Run relevant logic cycle.
        base.LateUpdate();
        UpdateStatusText();
        UpdateSurvivalChance();
        UpdateSurvivalChanceBar();
    }

    /// <summary>
    /// Updates the home priority based on the time of day.
    /// </summary>
    private void UpdateReturnHomePriority()
    {
        if(goals.ContainsKey(returnedGoal))
        {
            goals[returnedGoal] = GWorld.Instance.GetWorld().HasState("IsNight") ? returnHomePriority.Maximum : returnHomePriority.Minimum;
        }
    }

    /// <summary>
    /// Updates the status text to show the agents current action.
    /// </summary>
    private void UpdateStatusText()
    {
        // Null check, fails silently as a status text might not be wanted for all agents.
        if (statusText != null && currentAction != null)
        {
            // Set text to current action.
            statusText.text = currentAction.actionName;
        }
        else if(statusText != null)
        {
            // Set text to inactive.
            statusText.text = "Inactive";
        }
    }

    /// <summary>
    /// Updates the slider to show the agents current likelihood of survival.
    /// </summary>
    private void UpdateSurvivalChanceBar()
    {
        // Null check, fails silently as a status text might not exist for all agents.
        if (survivalBar != null)
        {
            // Update survival chance level.
            survivalBar.value = survivalChance;
        }
    }

    /// <summary>
    /// Copies the genes from a given struct.
    /// </summary>
    /// <param name="givenGenes">The genes being copied over.</param>
    public void SetBaseGenes(SurvivalGenes givenGenes)
    {
        agentHardinessOverSpeed = givenGenes.agentHardinessOverSpeed;
        agentGenerosity = givenGenes.agentGenerosity;
    }

    /// <summary>
    /// Updates the genes based on a given mutation range.
    /// </summary>
    /// <param name="mutationRange">The offset to apply to the genes.</param>
    public void MutateGenes(float mutationRange)
    {
        // Temporary reference to the manager.
        SurvivalSimulationManager SSM = SurvivalSimulationManager.SingletonManager;

        // Apply a ranged offset to the genes in a random direction.
        agentHardinessOverSpeed = Mathf.Clamp01(agentHardinessOverSpeed + Random.Range(-mutationRange, mutationRange));
        agentGenerosity = Mathf.Clamp01(agentGenerosity + Random.Range(-mutationRange, mutationRange));

        // Set the final speed.
        finalSpeed = baseSpeed + Mathf.Abs(SSM.hardinessSpeedCurve.Evaluate(agentHardinessOverSpeed) - 1) * SSM.speedInfluenceChange;
        navAgent.speed = finalSpeed;
    }

    /// <summary>
    /// Updates the survival chance based on food count and hardiness.
    /// </summary>
    public void UpdateSurvivalChance()
    {
        // Temporary reference to the manager.
        SurvivalSimulationManager SSM = SurvivalSimulationManager.SingletonManager;

        float penalties = 0;
        if (!beliefs.HasState("HasFood"))
        {
            penalties -= SSM.hungerPenalty;
        }
        if (!beliefs.HasState("ReturnedHome") && GWorld.Instance.GetWorld().HasState("IsNight"))
        {
            penalties -= SSM.darknessPenalty;
        }

        float foodChance = SSM.foodCurve.Evaluate((float)inventory.TotalFood / SSM.foodCapacity) * SSM.foodSurvivalBoost;
        float hardinessChance = SSM.hardinessSpeedCurve.Evaluate(agentHardinessOverSpeed) * SSM.hardinessSurvivalBoost;
        survivalChance = SSM.baseSurvivalChance + foodChance + hardinessChance + penalties;
    }

    /// <summary>
    /// Rolls to check if the agent has died based on their survival chance.
    /// </summary>
    /// <returns>If the agent should survive.</returns>
    public bool RollForSurvival() => ExtensionMethods.ProbabilityCheck(survivalChance / 100);

    /// <summary>
    /// Rolls to check if the agent has died based on their survival chance.
    /// </summary>
    /// <returns>If the agent will share.</returns>
    public bool RollForGenerosity() => ExtensionMethods.ProbabilityCheck(agentGenerosity);

    /// <summary>
    /// Checks if the agent can be generous, defined by if they have more than one piece of food.
    /// </summary>
    /// <returns>If the agent can share.</returns>
    public bool CanBeGenerous() => inventory.TotalFood > 1;

    /// <summary>
    /// Checks if the agent is starving with no food.
    /// </summary>
    /// <returns>If the agent has no food.</returns>
    public bool IsStarving() => inventory.TotalFood == 0;
}