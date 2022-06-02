using System;
using UnityEngine;

/// <summary>
/// Struct in charge of storing all relevant genes from an agent and is used to pass information to new agents.
/// </summary>
[Serializable]
public struct SurvivalGenes
{
    /// <summary>
    /// The natural hardiness of the agent, granting a higher base chance of survival over
    /// the natural speed of the agent, granting a higher movement speed.
    /// </summary>
    [Range(0, 1)]
    public float agentHardinessOverSpeed;

    /// <summary>
    /// The likelihood an agent will share excess food at the end of the day.
    /// </summary>
    [Range(0, 1)]
    public float agentGenerosity;

    /// <summary>
    /// Constructor for quick creation of the struct.
    /// </summary>
    /// <param name="agentHardinessOverSpeed">The ratio between agent speed and hardiness.</param>
    /// <param name="agentGenerosity">How likely an agent is to gift food.</param>
    public SurvivalGenes(float agentHardinessOverSpeed, float agentGenerosity)
    {
        this.agentHardinessOverSpeed = agentHardinessOverSpeed;
        this.agentGenerosity = agentGenerosity;
    }

    /// <summary>
    /// Constructor for quick creation of the struct.
    /// </summary>
    /// <param name="givenGenes">The genes being copied over.</param>
    public SurvivalGenes(SurvivalGenes givenGenes)
    {
        this.agentHardinessOverSpeed = givenGenes.agentHardinessOverSpeed;
        this.agentGenerosity = givenGenes.agentGenerosity;
    }
}