using UnityEngine;

public class FollowPlayerX : MonoBehaviour
{
    [Header("Camera Follow Settings")]
    public GameObject plane;   // Assign your plane in the Inspector
    private Vector3 offset;    // The distance between camera and plane

    void Start()
    {
        // Calculate the initial offset between the camera and the plane
        offset = transform.position - plane.transform.position;
    }

    void LateUpdate()
    {
        // Update camera position to follow the plane
        transform.position = plane.transform.position + offset;
    }
}
