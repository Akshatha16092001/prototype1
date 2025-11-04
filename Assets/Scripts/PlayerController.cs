using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private float horizontalInput;
    private float forwardInput;

    [Header("Movement Settings")]
    public float baseSpeed = 10f;               // Starting speed
    public float speedIncreaseRate = 0.3f;      // How fast speed increases over time
    public float maxSpeed = 25f;                // Maximum forward speed
    public float turnSpeed = 50f;
    private float currentSpeed;

    public bool canMove = true;

    [Header("Sound Settings")]
    public AudioClip engineSound;
    public AudioClip crashSound;
    private AudioSource playerAudio;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        currentSpeed = baseSpeed;

        playerAudio = GetComponent<AudioSource>();
        if (playerAudio == null)
            playerAudio = gameObject.AddComponent<AudioSource>();

        if (engineSound != null)
        {
            playerAudio.clip = engineSound;
            playerAudio.loop = true;
            playerAudio.Play();
        }
    }

    void FixedUpdate() // ✅ Physics-based movement
    {
        if (!canMove)
        {
            if (playerAudio.isPlaying && playerAudio.clip == engineSound)
                playerAudio.Pause();
            return;
        }

        // 🏎️ Gradually increase speed over time
        currentSpeed += speedIncreaseRate * Time.fixedDeltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, baseSpeed, maxSpeed);

        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        // Move forward automatically OR with player input
        float effectiveSpeed = currentSpeed * Mathf.Max(0.2f, forwardInput); // slight slowdown if not pressing forward
        Vector3 move = transform.forward * effectiveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        // Rotate car
        Quaternion turn = Quaternion.Euler(0, horizontalInput * turnSpeed * Time.fixedDeltaTime, 0);
        rb.MoveRotation(rb.rotation * turn);

        // 🎵 Adjust engine pitch with speed
        if (playerAudio.clip == engineSound)
            playerAudio.pitch = 1f + (currentSpeed / maxSpeed) * 0.5f;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Player hit an obstacle! Game Over triggered.");
            canMove = false;

            if (playerAudio.clip == engineSound)
                playerAudio.Stop();

            if (crashSound != null)
                playerAudio.PlayOneShot(crashSound, 1.0f);

            FindObjectOfType<GameManager>().SendMessage("GameOver");
        }
    }
}
