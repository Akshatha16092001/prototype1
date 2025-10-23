using UnityEngine;

public class PlayerControllerX : MonoBehaviour
{
    [Header("Plane Settings")]
    public float speed = 15f;           // Forward movement speed
    public float rotationSpeed = 45f;   // Turning & tilting speed

    private float verticalInput;        // Up/down input (arrow keys or W/S)
    private float horizontalInput;      // Left/right input (arrow keys or A/D)

    void Update()
    {
        // --- Get player input ---
        verticalInput = Input.GetAxis("Vertical");     // W/S or Up/Down
        horizontalInput = Input.GetAxis("Horizontal"); // A/D or Left/Right

        // --- Move plane forward constantly (independent of input) ---
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // --- Tilt plane up/down only if vertical input is pressed ---
        if (Mathf.Abs(verticalInput) > 0.01f)
        {
            // Tilt the plane around the X axis (pitch) based on vertical input
            transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime * -verticalInput);
            // Negative sign so pressing up tilts the nose up
        }
        else
        {
            // Optional: Gradually level the plane when no vertical input
            // Smoothly rotate back towards zero tilt on X axis
            Vector3 currentRotation = transform.rotation.eulerAngles;
            float tiltX = currentRotation.x;

            // Convert angles > 180 to negative for smooth interpolation
            if (tiltX > 180f) tiltX -= 360f;

            float tiltCorrection = Mathf.Lerp(tiltX, 0f, 2f * Time.deltaTime);
            transform.rotation = Quaternion.Euler(tiltCorrection, currentRotation.y, currentRotation.z);
        }

        // --- Turn plane left/right based on horizontal input ---
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime * horizontalInput);
    }
}
