using UnityEngine;

/// <summary>
/// Visualizer component that allows for agent data to be displayed.
/// Functions in combination with the <see cref="GAgentVisualEditor"/> custom editor.
/// </summary>
[ExecuteInEditMode]
public class GAgentVisual : MonoBehaviour
{
    /// <summary>
    /// The agent being displayed.
    /// </summary>
    public GAgent thisAgent;

    /// <summary>
    /// Gets the component in case one is not set.
    /// </summary>
    private void Start() => thisAgent = this.GetComponent<GAgent>();
}
