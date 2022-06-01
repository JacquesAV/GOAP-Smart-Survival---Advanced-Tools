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
    public int returnHomePriority = 20;

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

    [Header("Miscellaneous")]
    /// <summary>
    /// Boolean to track if the agent is considered dead or alive.
    /// </summary>
    public bool isDead = true;

    /// <summary>
    /// Boolean to track if the agent is considered dead or alive.
    /// </summary>
    public bool isAsleep = true;

    /// <summary>
    /// Boolean to track if the agent had gifted food.
    /// </summary>
    public bool wasGenerous = true;

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
    /// Save the current survival simulation manager locally for ease of use.
    /// </summary>
    private readonly SurvivalSimulationManager SSM = SurvivalSimulationManager.SingletonManager;

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
        if (survivalBar != null) { survivalBar.maxValue = survivalChance; }

        // Create & add goals with priorities, set to false to keep goal after completion.
        SubGoal collectionGoal = new SubGoal("CollectedFood", 1, false);
        goals.Add(collectionGoal, foodCollectionPriority);

        SubGoal returnedGoal = new SubGoal("ReturnedHome", 1, true);
        goals.Add(returnedGoal, returnHomePriority);
    }

    /// <summary>
    /// Late update visual elements.
    /// </summary>
    new void LateUpdate()
    {
        // Return early if agent is asleep.
        if (isAsleep) return;

        base.LateUpdate();
        UpdateStatusText();
        UpdateSurvivalChance();
        UpdateSurvivalChanceBar();
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
    /// Updates the genes based on a given mutation range.
    /// </summary>
    /// <param name="mutationRange">The offset to apply to the genes.</param>
    public void MutateGenes(float mutationRange)
    {
        // Apply a ranged offset to the genes in a random direction.
        agentGenerosity = Mathf.Clamp01(agentGenerosity + Random.Range(-mutationRange, mutationRange));
        agentHardinessOverSpeed = Mathf.Clamp01(agentHardinessOverSpeed + Random.Range(-mutationRange, mutationRange));

        // Set the final speed.
        finalSpeed = Mathf.Abs(SSM.hardinessSpeedCurve.Evaluate(agentHardinessOverSpeed) - 1) * SSM.speedInfluenceChange;
        navAgent.speed = finalSpeed;
    }

    /// <summary>
    /// Updates the survival chance based on food count and hardiness.
    /// </summary>
    public void UpdateSurvivalChance()
    {
        float foodChance = SSM.foodCurve.Evaluate((float)inventory.TotalFood / SSM.foodCapacity) * SSM.foodSurvivalBoost;
        float hardinessChance = SSM.hardinessSpeedCurve.Evaluate(agentHardinessOverSpeed) * SSM.hardinessSurvivalBoost;
        survivalChance = SSM.baseSurvivalChance + foodChance + hardinessChance;
    }

    /// <summary>
    /// Rolls to check if the agent has died based on their survival chance
    /// </summary>
    /// <returns>If the agent has died.</returns>
    public bool RollForIsDead()
    {
        bool temporaryIsDead = ExtensionMethods.ProbabilityCheck(survivalChance / 100);
        isDead = temporaryIsDead;
        return temporaryIsDead;
    }
}