using UnityEngine;

public class PlayerControllerX : MonoBehaviour
{
    [Header("Plane Settings")]
    public float speed = 15f;           // Forward movement speed
    public float rotationSpeed = 45f;   // Turning & tilting speed

    private float verticalInput;        // Up/down input (arrow keys or W/S)
    private float horizontalInput;      // Left/right input (arrow keys or A/D)
    void Start()
    {
        
    }

    void Update()
    {
        // --- Get player input ---
        verticalInput = Input.GetAxis("Vertical");     // W/S or Up/Down
        horizontalInput = Input.GetAxis("Horizontal"); // A/D or Left/Right

        // --- Move plane forward constantly ---
        transform.Translate(Vector3.forward * speed * Time.deltaTime * verticalInput);

        // --- Tilt plane up/down based on vertical input ---
        //transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime * verticalInput);

        // --- Turn plane left/right based on horizontal input ---
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime * horizontalInput);
    }
}
