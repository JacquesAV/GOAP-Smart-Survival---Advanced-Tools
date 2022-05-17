using System.Collections.Generic;

/// <summary>
/// A world state that an agent can check.
/// </summary>
[System.Serializable]
public class WorldState
{
    /// <summary>
    /// The name of the state.
    /// </summary>
    public string key;

    /// <summary>
    /// The current value assosiated to the state.
    /// </summary>
    public int value;
}

/// <summary>
/// The states of the world that an agent can check.
/// </summary>
public class WorldStates
{
    /// <summary>
    /// Links the states together.
    /// </summary>
    public Dictionary<string, int> States { get; private set; }

    /// <summary>
    /// Constructor for quick creation of class.
    /// </summary>
    public WorldStates() => States = new Dictionary<string, int>();

    /// <summary>
    /// Check for if the state exists.
    /// </summary>
    /// <param name="key">The state name being checked for.</param>
    /// <returns>If the world states contains a given state.</returns>
    public bool HasState(string key) => States.ContainsKey(key);

    /// <summary>
    /// Add a state and assosiated key to the dictionary.
    /// </summary>
    /// <param name="key">The name of the state being added.</param>
    /// <param name="value">The value assosiated with the state.</param>
    public void AddState(string key, int value) => States.Add(key, value);

    /// <summary>
    /// Modify an assosiated dictinary entrys value.
    /// </summary>
    /// <param name="key">The name of the state being modified.</param>
    /// <param name="value">The value assosiated with the state.</param>
    public void ModifyState(string key, int value)
    {
        if (States.ContainsKey(key))
        {
            States[key] += value;

            // Prevents negatives.
            if (States[key] <= 0)
            {
                RemoveState(key);
            }
        }
        else
        {
            AddState(key, value);
        }
    }

    /// <summary>
    /// Remove the key pair from the dictionary.
    /// </summary>
    /// <param name="key">The state being removed.</param>
    public void RemoveState(string key)
    {
        if (States.ContainsKey(key))
        {
            States.Remove(key);
        }
    }

    /// <summary>
    /// Set the value to a state.
    /// </summary>
    /// <param name="key">The name of the state being set.</param>
    /// <param name="value">The value assosiated with the state.</param>
    public void SetState(string key, int value)
    {
        if (States.ContainsKey(key))
        {
            States[key] = value;
        }
        else
        {
            States.Add(key, value);
        }
    }
}
