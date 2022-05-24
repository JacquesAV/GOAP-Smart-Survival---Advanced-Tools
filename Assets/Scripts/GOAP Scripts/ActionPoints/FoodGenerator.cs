using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that handles the spawning of food within a given area.
/// It will handle and avoid overlapping so that one given cell only spawns a single piece of food.
/// </summary>
public class FoodGenerator : MonoBehaviour
{
    /// <summary>
    /// The prefab that will be used for food spawned.
    /// </summary>
    public GameObject foodPrefab;

    /// <summary>
    /// The amount of food to spawn.
    /// </summary>
    public int foodCount;

    /// <summary>
    /// The dimensions to define the area in which food should be spawned.
    /// </summary>
    public Vector2Int dimensions;

    /// <summary>
    /// Grid that represents where food should be created.
    /// </summary>
    public bool[,] foodSpawnGrid;

    /// <summary>
    /// Grid that represents where food is being created.
    /// </summary>
    public List<GameObject> foodList = new List<GameObject>();

    /// <summary>
    /// Generate food points within the spawn region.
    /// </summary>
    public void GenerateFoodInArea()
    {
        // Setup the dimensions of the food data grid.
        foodSpawnGrid = new bool[dimensions.x, dimensions.y];

        // Generate a list of available positions.
        List<Vector2Int> availablePositions = new List<Vector2Int>();
        for (int x = 0; x < foodSpawnGrid.GetLength(0); x++)
        {
            for (int y = 0; y < foodSpawnGrid.GetLength(1); y++)
            {
                foodSpawnGrid[x, y] = false;
                availablePositions.Add(new Vector2Int(x, y));
            }
        }

        // Mark random positions in the grid as food.
        for (int i = 0; i < foodCount; i++)
        {
            Vector2Int randomPosition = availablePositions[Random.Range(0, availablePositions.Count)];
            availablePositions.Remove(randomPosition);
            foodSpawnGrid[randomPosition.x, randomPosition.y] = true;
        }

        // Generate food based on the marked locations.
        for (int x = 0; x < foodSpawnGrid.GetLength(0); x++)
        {
            for (int y = 0; y < foodSpawnGrid.GetLength(1); y++)
            {
                // Spawn food if marked for spawning.
                if (foodSpawnGrid[x, y])
                {
                    // Instantiate and save the food.
                    GameObject food = Instantiate(foodPrefab, this.transform);
                    foodList.Add(food);

                    // Place the food on the intended grid position around the generator.
                    food.transform.position = this.transform.position + new Vector3Int((-dimensions.x / 2) + x, (-dimensions.y / 2) + y, 0);
                }
            }
        }
    }

    /// <summary>
    /// Removes/destroys all remaining food being tracked.
    /// </summary>
    public void RemoveFood()
    {
        for (int i = foodList.Count - 1; i >= 0; i--)
        {
            Destroy(foodList[i]);
        }
        foodList.Clear();
    }

    /// <summary>
    /// Draw a visual representation of the area in which food will be generated.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(this.transform.position, new Vector3(dimensions.x, dimensions.y, 0));
    }
}
