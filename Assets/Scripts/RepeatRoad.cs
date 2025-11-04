using UnityEngine;

public class RepeatRoad : MonoBehaviour
{
    private Vector3 startPos;
    private float repeatLength;

    void Start()
    {
        // Store the starting position of the road
        startPos = transform.position;

        // Get half the length of the road from its BoxCollider (along Z-axis)
        repeatLength = GetComponent<BoxCollider>().size.z / 2;
    }

    void Update()
    {
        // When the road moves behind the player, reset its position
        if (transform.position.z < startPos.z - repeatLength)
        {
            transform.position = startPos;
        }
    }
}
