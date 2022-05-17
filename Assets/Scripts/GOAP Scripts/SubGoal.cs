using System.Collections.Generic;

/// <summary>
/// Data class to track goal data.
/// </summary>
public class SubGoal
{
    /// <summary>
    /// Dictionary to store the goals.
    /// </summary>
    public Dictionary<string, int> sGoals;

    /// <summary>
    /// Bool to store if a given goal should be removed upon completion.
    /// </summary>
    public bool isRemovedOnCompletion;

    /// <summary>
    /// Constructor for quick creation of a goal.
    /// </summary>
    /// <param name="goalName">The name that the goal should be given.</param>
    /// <param name="valuePair">How valuable the goal is considered to be.</param>
    /// <param name="shouldRemoveOnCompletion">If the goal should be removed upon completion.</param>
    public SubGoal(string goalName, int valuePair, bool shouldRemoveOnCompletion)
    {
        sGoals = new Dictionary<string, int>();
        sGoals.Add(goalName, valuePair);
        isRemovedOnCompletion = shouldRemoveOnCompletion;
    }
}
