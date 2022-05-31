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
    /// Bool to flip the spawning location of the agents.
    /// </summary>
    public bool verticalFlip;

    /// <summary>
    /// Bool to flip the spawning location of the agents.
    /// </summary>
    public bool horizontalFlip;

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
            if(verticalFlip)
            {
                //Generate food based on the marked locations.
                for (int x = dimensions.x - 1; x >= 0; x--)
                {
                    if (horizontalFlip)
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
                            GameObject agent = Instantiate(currentAgent, this.transform.position + new Vector3Int((-dimensions.x / 2) + x, (-dimensions.y / 2) + y, 0), Quaternion.identity);
                            instantiatedAgents.Add(agent);
                        }
                    }
                    else
                    {
                        for (int y = dimensions.y - 1; y >= 0; y--)
                        {
                            // Return early with list of instantiated agents if done with spawning.
                            if (agentsToSpawn.Count == 0)
                            {
                                return instantiatedAgents;
                            }

                            // Dequeue the current agent to spawn.
                            GameObject currentAgent = agentsToSpawn.Dequeue();

                            // Instantiate and save the agent.
                            GameObject agent = Instantiate(currentAgent, this.transform.position + new Vector3Int((-dimensions.x / 2) + x, (-dimensions.y / 2) + y, 0), Quaternion.identity);
                            instantiatedAgents.Add(agent);
                        }
                    }
                }
            }
            else
            {
                //Generate food based on the marked locations.
                for (int x = 0; x < dimensions.x; x++)
                {
                    if (horizontalFlip)
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
                            GameObject agent = Instantiate(currentAgent, this.transform.position + new Vector3Int((-dimensions.x / 2) + x, (-dimensions.y / 2) + y, 0), Quaternion.identity);
                            instantiatedAgents.Add(agent);
                        }
                    }
                    else
                    {
                        for (int y = dimensions.y - 1; y >= 0; y--)
                        {
                            // Return early with list of instantiated agents if done with spawning.
                            if (agentsToSpawn.Count == 0)
                            {
                                return instantiatedAgents;
                            }

                            // Dequeue the current agent to spawn.
                            GameObject currentAgent = agentsToSpawn.Dequeue();

                            // Instantiate and save the agent.
                            GameObject agent = Instantiate(currentAgent, this.transform.position + new Vector3Int((-dimensions.x / 2) + x, (-dimensions.y / 2) + y, 0), Quaternion.identity);
                            instantiatedAgents.Add(agent);
                        }
                    }
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
