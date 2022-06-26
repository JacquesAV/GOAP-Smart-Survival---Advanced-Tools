using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// A CSV writer editor window for the simulation capable of exporting relevant <see cref="SurvivalAgent"/> prefab data.
/// </summary>
public class SimulationCSVWriter : EditorWindow
{
    /// <summary>
    /// The name and path for the exported CSV file.
    /// </summary>
    private string exportFilePath = "Assets/ExampleCSV.csv";

    /// <summary>
    /// The path for the simulations generations.
    /// </summary>
    private string primarySimulationsPath = "Assets/Simulations";

    /// <summary>
    /// The type of agent to export, based on their survival result.
    /// </summary>
    private SurvivalState survivalStateFilter;

    /// <summary>
    /// A toggle that allows for swapping between the USA/UK and European format.
    /// </summary>
    private bool usingEuropeanFormat = true;

    /// <summary>
    /// Constant value for shifting labels in the GUI, measured in pixels.
    /// </summary>
    private const float LABEL_SHIFT = 105;

    /// <summary>
    /// Defines a minimum size for the editor window in width and height.
    /// </summary>
    private static readonly Vector2 _editorMinimumSize = new Vector2(200, 125);

    /// <summary>
    /// Custom menu item for opening a minimum size enforced editor window.
    /// </summary>
    /// <returns>A minimum size enforced editor window.</returns>
    [MenuItem("Tools/CSV Prefab Exporter")]
    public static SimulationCSVWriter GetSizedWindow()
    {
        // Gets the window instance.
        SimulationCSVWriter sizedWindow = GetWindow<SimulationCSVWriter>("CSV Prefab Exporter");

        // Sets the minimum size.
        sizedWindow.minSize = _editorMinimumSize;

        // Returns the sized window.
        return sizedWindow;
    }

    /// <summary>
    /// Calls for the rendering and handling of custom and default GUI events.
    /// </summary>
    private void OnGUI()
    {
        // Validate the file path end.
        if (!exportFilePath.EndsWith(".csv"))
        {
            exportFilePath += ".csv";
        }

        // Title label for the editor.
        GUILayout.Label("Export Specifications", EditorStyles.boldLabel);

        // Change the width of labels to better fit the editor window.
        EditorGUIUtility.labelWidth = LABEL_SHIFT;

        // Allow for modification of the name and path of the exported CSV file.
        exportFilePath = EditorGUILayout.TextField("CSV Export Path: ", exportFilePath);

        // Allow for modification of the simulation path to export.
        primarySimulationsPath = EditorGUILayout.TextField("Simulation Path: ", primarySimulationsPath);

        // Allow for modification of the type of agent to export.
        survivalStateFilter = (SurvivalState)EditorGUILayout.EnumPopup("State Filter: ", survivalStateFilter);

        // Allow for modification of the export format type.
        usingEuropeanFormat = EditorGUILayout.Toggle("European Format: ", usingEuropeanFormat);

        // Button for exporting the given simulation path.
        if (GUILayout.Button("Save Agent Data to File", GUILayout.Height(25)))
        {
            WriteCSV();
        }
    }

    /// <summary>
    /// Writes the relevant simulation data to a CSV file.
    /// </summary>
    private void WriteCSV()
    {
        // Fetch the asset GUIDs of all prefabs found within the given path.
        string[] assetGUIDs = AssetDatabase.FindAssets("t:prefab", new string[] { primarySimulationsPath });

        // Temporary list to hold references to the survival agents that will be exported.
        List<SurvivalAgent> agentsToExport = new List<SurvivalAgent>();

        // Define the separator and decimal types.
        string separatorSymbol = usingEuropeanFormat ? "; " : ", ";

        // Iterate over each found GUID.
        foreach (string guid in assetGUIDs)
        {
            // Find the prefab that the GUID points to.
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject assetObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            // If the prefab is a survival agent and meets the survival state filter, add to the export list.
            if (assetObject.TryGetComponent(out SurvivalAgent assetAgent) && (survivalStateFilter == SurvivalState.UNDEFINED || assetAgent.survivalState == survivalStateFilter))
            {
                if(assetAgent.survivalState == SurvivalState.UNDEFINED)
                {
                    Debug.LogWarning("Agent being exported was found too be undefined, caution!");
                }
                agentsToExport.Add(assetAgent);
            }
        }

        // Begin exporting the agents to a CVS file.
        if(agentsToExport.Count > 0)
        {
            // Debug the results.
            Debug.Log(string.Concat("Exporting ", agentsToExport.Count, " agents to CSV file!"));

            // Open a stream for the text writer class.
            TextWriter textWriter = new StreamWriter(exportFilePath, true);

            // Build and format the string with the header information.
            List<string> headerData = new List<string> { "Generation", "Survival State", "Survival Chance", "Total Food", "Hardiness over Speed", "Generosity", "Was Generous" };
            string formattedHeaderData = string.Join(separatorSymbol, headerData);

            // Write the string to the file.
            textWriter.WriteLine(formattedHeaderData);

            // Declare local variables for the agents to set.
            string generation;
            string survivalState;
            string survivalChance;
            string totalFood;
            string generous;
            string generosity;
            string hardinessOverSpeed;

            // Iterate over each agent to export and write it to the file.
            foreach (SurvivalAgent item in agentsToExport)
            {
                // Update the local variables.
                generation = item.generationNumber.ToString();
                survivalState = item.survivalState.ToString();
                survivalChance = item.SurvivalChance.ToString();
                totalFood = item.inventory.TotalFood.ToString();
                generous = item.wasGenerous.ToString();
                hardinessOverSpeed = item.agentHardinessOverSpeed.ToString();
                generosity = item.agentGenerosity.ToString();

                // Replace decimal symbols if applicable.
                if(usingEuropeanFormat)
                {
                    survivalChance = survivalChance.Replace(".",",");
                    hardinessOverSpeed = hardinessOverSpeed.Replace(".", ",");
                    generosity = generosity.Replace(".", ",");
                }

                // Build and format the string with the ordered information.
                List<string> agentData = new List<string> { generation, survivalState, survivalChance, totalFood, hardinessOverSpeed, generosity, generous };
                string formattedData = string.Join(separatorSymbol, agentData);

                // Write the string to the file.
                textWriter.WriteLine(formattedData);
            }

            // Close the text writing.
            textWriter.Close();
        }
    }
}