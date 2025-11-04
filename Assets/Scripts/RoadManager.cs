using UnityEngine;
using System.Collections.Generic;

public class RoadManager : MonoBehaviour
{
    public GameObject roadPrefab;     // Assign your road tile prefab here
    public int numberOfRoads = 3;     // How many tiles to keep active
    public float roadLength = 50f;    // Length of each road tile (Z size)
    public Transform player;          // Reference to player

    private List<GameObject> activeRoads = new List<GameObject>();
    private float spawnZ = 0f;        // Where the next tile spawns
    private float safeZone = 60f;     // Distance before recycling

    void Start()
    {
        for (int i = 0; i < numberOfRoads; i++)
        {
            SpawnRoad();
        }
    }

    void Update()
    {
        // If player has moved far enough, recycle the first tile
        if (player.position.z - safeZone > (spawnZ - numberOfRoads * roadLength))
        {
            RecycleRoad();
        }
    }

    void SpawnRoad()
    {
        GameObject go = Instantiate(roadPrefab, Vector3.forward * spawnZ, Quaternion.identity);
        activeRoads.Add(go);
        spawnZ += roadLength;
    }

    void RecycleRoad()
    {
        // Move the first road tile to the end
        GameObject firstRoad = activeRoads[0];
        activeRoads.RemoveAt(0);
        firstRoad.transform.position = Vector3.forward * spawnZ;
        spawnZ += roadLength;
        activeRoads.Add(firstRoad);
    }
}
