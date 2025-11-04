using UnityEngine;

public class RoadManager : MonoBehaviour
{
    [Header("References")]
    public GameObject roadPrefab;   // Your Road prefab (with rotation -0,90,0)
    public Transform player;        // Player (car)

    [Header("Settings")]
    public int initialRoadCount = 3; // How many roads to start with
    public float roadLength = 30f;   // Length of one road piece
    public float spawnDistance = 60f; // How far ahead to spawn next road

    private float nextSpawnZ = 0f;   // Z position tracker (not global)
    private GameObject lastRoad;     // Keep track of the last spawned road

    void Start()
    {
        // Find player automatically if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogError("RoadManager: Player not found! Assign it in Inspector or tag it 'Player'.");
        }

        // Spawn initial roads
        for (int i = 0; i < initialRoadCount; i++)
        {
            SpawnRoad();
        }
    }

    void Update()
    {
        if (player == null) return;

        // Spawn a new road when the player approaches the end of the current one
        if (player.position.z + spawnDistance > nextSpawnZ)
        {
            SpawnRoad();
        }
    }

    void SpawnRoad()
    {
        Vector3 spawnPos;
        Quaternion spawnRot;

        if (lastRoad == null)
        {
            // First road at origin
            spawnPos = Vector3.zero;
            spawnRot = Quaternion.Euler(0, -90, 0); // Match your road prefab’s rotation (-0,90,0)
        }
        else
        {
            // Spawn next road right after the previous one
            spawnRot = lastRoad.transform.rotation;
            spawnPos = lastRoad.transform.position + lastRoad.transform.forward * roadLength;
        }

        GameObject newRoad = Instantiate(roadPrefab, spawnPos, spawnRot, transform);
        lastRoad = newRoad;

        nextSpawnZ += roadLength;
    }
}
