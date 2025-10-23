using UnityEngine;

public class FollowPlayerX : MonoBehaviour
{
    [Header("Camera Follow Settings")]
    public GameObject plane; // Assign this in Inspector

    private Vector3 offset;

    void Start()
    {
        // Position behind and above the plane
        offset = new Vector3(0, 5, -15);
    }

    void LateUpdate()
    {
        if (plane == null) return; // Safety check

        // Move camera to plane position + offset
        transform.position = plane.transform.position + offset;

        // Rotate camera to look at plane
        transform.LookAt(plane.transform);
    }
}
