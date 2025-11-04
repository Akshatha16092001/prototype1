using UnityEngine;

public class RepeatRoad : MonoBehaviour
{
    public Transform player; // Drag your car/player here in Inspector

    private float roadLength;
    private Vector3 startPos;

    void Start()
    {
        // Store starting position
        startPos = transform.position;

        // Get full length of the road mesh (Z-axis)
        roadLength = GetComponent<BoxCollider>().size.z;
    }

    void Update()
    {
        // If player has moved past one road length ahead of this road
        if (player.position.z - transform.position.z > roadLength)
        {
            // Move the road ahead by twice its length (creates looping effect)
            transform.position += new Vector3(0, 0, roadLength * 2);
        }
    }
}
