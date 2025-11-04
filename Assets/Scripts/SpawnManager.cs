

using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public GameObject obstaclePrefab;   // Drag your Obstacle prefab here
    public int obstacleCount = 10;      // Number of obstacles to spawn
    public float spawnZStart = 20f;     // Start of the spawning area (Z position)
    public float spawnZEnd = 180f;      // End of the spawning area (Z position)
    public float laneWidth = 3f;        // How wide obstacles can appear (X range)
    public float obstacleY = 0.5f;      // Height from ground

    void Start()
    {
        SpawnObstacles();
    }

    void SpawnObstacles()
    {
        // Safety check
        if (obstaclePrefab == null)
        {
            Debug.LogWarning("SpawnManager: Obstacle prefab not assigned!");
            return;
        }

        for (int i = 0; i < obstacleCount; i++)
        {
            // Random Z position along the road
            float zPos = Random.Range(spawnZStart, spawnZEnd);

            // Random X position (lane left or right)
            float xPos = Random.Range(-laneWidth, laneWidth);

            // Random rotation for variety
            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            // Final position
            Vector3 spawnPos = new Vector3(xPos, obstacleY, zPos);

            // Create the obstacle
            Instantiate(obstaclePrefab, spawnPos, randomRotation);
        }

        Debug.Log("SpawnManager: Spawned " + obstacleCount + " obstacles between Z=" + spawnZStart + " and Z=" + spawnZEnd);
    }
}
