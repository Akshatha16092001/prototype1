using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private float horizontalInput;
    private float forwardInput;

    [Header("Movement Settings")]
    public float baseSpeed = 15f;
    public float speedIncreaseRate = 0.3f;
    public float maxSpeed = 25f;
    public float turnSpeed = 70f;
    public bool canMove = true;

    private float currentSpeed;
    public ParticleSystem dustTrail;

    [Header("Sound Settings")]
    public AudioClip engineSound;
    public AudioClip crashSound;
    private AudioSource playerAudio;

    private float fixedY; // Keeps car on road

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        fixedY = rb.position.y; // Store starting Y position
        currentSpeed = baseSpeed;

        playerAudio = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        if (engineSound != null)
        {
            playerAudio.clip = engineSound;
            playerAudio.loop = true;
            playerAudio.Play();
        }
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        // ------------------------
        // INPUT
        // ------------------------
        if (MobileInput.horizontal != 0f || MobileInput.vertical != 0f)
        {
            horizontalInput = MobileInput.horizontal;
            forwardInput = MobileInput.vertical;
        }
        else
        {
            horizontalInput = Input.GetAxis("Horizontal");
            forwardInput = Input.GetAxis("Vertical");
        }

        // ------------------------
        // SPEED
        // ------------------------
        currentSpeed += speedIncreaseRate * Time.fixedDeltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, baseSpeed, maxSpeed);

        // ------------------------
        // FORWARD MOVEMENT (physics-based)
        // ------------------------
        Vector3 forwardMove = transform.forward * currentSpeed * Mathf.Max(0.2f, forwardInput);
        Vector3 newVelocity = new Vector3(forwardMove.x, rb.velocity.y, forwardMove.z); // keep physics y (gravity/collision)
        rb.velocity = newVelocity;

        // ------------------------
        // TURNING
        // ------------------------
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            float turnAngle = horizontalInput * turnSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, turnAngle, 0));
        }

        // ------------------------
        // DUST TRAIL
        // ------------------------
        if (dustTrail != null)
        {
            var emission = dustTrail.emission;
            emission.enabled = Mathf.Abs(forwardInput) > 0.1f;
        }

        // ------------------------
        // ENGINE SOUND
        // ------------------------
        if (playerAudio.clip == engineSound)
            playerAudio.pitch = 1f + (currentSpeed / maxSpeed) * 0.5f;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            canMove = false;

            // Stop Rigidbody completely
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;

            // Stop engine sound and play crash sound
            if (playerAudio.clip == engineSound)
                playerAudio.Stop();

            if (crashSound != null)
                playerAudio.PlayOneShot(crashSound);

            // Trigger Game Over
            FindObjectOfType<GameManager>().SendMessage("GameOver");
        }
    }
}
