using System.Collections.Generic;

/// <summary>
/// Allows for the construction of a planning graph, they point to previous actions.
/// </summary>
public class Node
{
    /// <summary>
    /// The parent node assosiated with this instance.
    /// </summary>
    public Node parent;

    /// <summary>
    /// How expensive this action node is to follow.
    /// </summary>
    public float cost;

    /// <summary>
    /// DIctionary of current states relevant to this node.
    /// </summary>
    public Dictionary<string, int> state;

    /// <summary>
    /// The action assosiated with this node.
    /// </summary>
    public GAction action;

    /// <summary>
    /// Constructor for quick creation of the node.
    /// </summary>
    /// <param name="parent">The parent node to assosiate with this child instance.</param>
    /// <param name="cost">How expensive this node is to perform.</param>
    /// <param name="allStates">The states assosiated with this node.</param>
    /// <param name="action">The action assosiated with this node.</param>
    public Node(Node parent, float cost, Dictionary<string, int> allStates, GAction action)
    {
        this.parent = parent;
        this.cost = cost;
        this.state = new Dictionary<string, int>(allStates);
        this.action = action;
    }

    /// <summary>
    /// Constructor for quick creation of the node, accepting all states and belief states.
    /// </summary>
    /// <param name="parent">The parent node to assosiate with this child instance.</param>
    /// <param name="cost">How expensive this node is to perform.</param>
    /// <param name="allStates">The states assosiated with this node.</param>
    /// <param name="beliefStates">The agents belief states assosiated with this node.</param>
    /// <param name="action">The action assosiated with this node.</param>
    public Node(Node parent, float cost, Dictionary<string, int> allStates, Dictionary<string, int> beliefStates, GAction action)
    {
        this.parent = parent;
        this.cost = cost;
        this.state = new Dictionary<string, int>(allStates);
        foreach (KeyValuePair<string, int> b in beliefStates)
        {
            if (!this.state.ContainsKey(b.Key))
            {
                this.state.Add(b.Key, b.Value);
            }
            this.action = action;
        }
    }
}