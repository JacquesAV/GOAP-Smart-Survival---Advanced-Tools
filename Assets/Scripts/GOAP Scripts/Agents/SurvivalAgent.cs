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
    /// The current survival chance that a given agent has.
    /// </summary>
    public float survivalChance;

    /// <summary>
    /// The natural speed of the agent, granting a higher movement speed.
    /// </summary>
    public float agentSpeed;

    /// <summary>
    /// The natural hardiness of the agent, granting a higher base chance of survival.
    /// </summary>
    public float agentHardiness;

    /// <summary>
    /// Text to visually show the status of the agent.
    /// </summary>
    public TextMeshProUGUI statusText = null;

    /// <summary>
    /// Slider to visually show the survival chance of the agent.
    /// </summary>
    public Slider survivalBar = null;

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
    /// Update visual elements.
    /// </summary>
    new void LateUpdate()
    {
        base.LateUpdate();
        UpdateStatusText();
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
}