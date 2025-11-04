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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

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
            if (playerAudio.isPlaying && playerAudio.clip == engineSound)
                playerAudio.Pause();
            return;
        }

        currentSpeed += speedIncreaseRate * Time.fixedDeltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, baseSpeed, maxSpeed);

        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        float effectiveSpeed = currentSpeed * Mathf.Max(0.2f, forwardInput);
        Vector3 move = transform.forward * effectiveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        Quaternion turn = Quaternion.Euler(0, horizontalInput * turnSpeed * Time.fixedDeltaTime, 0);
        rb.MoveRotation(rb.rotation * turn);

        if (playerAudio.clip == engineSound)
            playerAudio.pitch = 1f + (currentSpeed / maxSpeed) * 0.5f;

        if (dustTrail != null)
        {
            var emission = dustTrail.emission;
            emission.enabled = Mathf.Abs(forwardInput) > 0.1f;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            canMove = false;

            if (playerAudio.clip == engineSound)
                playerAudio.Stop();

            if (crashSound != null)
                playerAudio.PlayOneShot(crashSound);

            FindObjectOfType<GameManager>().SendMessage("GameOver");
        }
    }
}
