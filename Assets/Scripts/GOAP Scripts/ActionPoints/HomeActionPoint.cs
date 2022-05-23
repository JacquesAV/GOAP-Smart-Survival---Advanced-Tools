using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Action point that manages the creation of a given agent type.
/// </summary>
public class HomeActionPoint : ActionPoint
{
    /// <summary>
    /// The dimensions to define the area in which agents should be spawned.
    /// </summary>
    [SerializeField]
    private Vector2Int dimensions;

    /// <summary>
    /// Generate food points within the spawn region.
    /// </summary>
    /// <param name="agents">The agents to create in the area.</param>
    /// <returns>The list of agents created so that they may externally be tracked.</returns>
    public List<GameObject> GenerateAgentsInArea(List<GameObject> agents)
    {
        // Shuffle the list of agents.
        agents.Shuffle();

        // Queue of agents for easy iteration.
        Queue<GameObject> agentsToSpawn = ExtensionMethods.ConvertListToQueue(agents);

        // List of instantiated agents.
        List<GameObject> instantiatedAgents = new List<GameObject>();

        // For as long as agents remain to be created, keep spawning within the designated area.
        while (agentsToSpawn.Count > 0)
        {
            //Generate food based on the marked locations.
            for (int x = 0; x < dimensions.x; x++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    // Return early with list of instantiated agents if done with spawning.
                    if (agentsToSpawn.Count == 0)
                    {
                        return instantiatedAgents;
                    }

                    // Dequeue the current agent to spawn.
                    GameObject currentAgent = agentsToSpawn.Dequeue();

                    // Instantiate and save the agent.
                    GameObject agent = Instantiate(currentAgent);
                    instantiatedAgents.Add(agent);

                    // Place the food on the intended grid position around the generator.
                    agent.transform.position = this.transform.position + new Vector3Int((-dimensions.x / 2) + x, (-dimensions.y / 2) + y, 0);
                }
            }
        }

        // Return the instantiated list of agents.
        return instantiatedAgents;
    }

    /// <summary>
    /// Draw a visual representation of the area in which agents will be generated.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(this.transform.position, new Vector3(dimensions.x, dimensions.y, 0));
    }
}
