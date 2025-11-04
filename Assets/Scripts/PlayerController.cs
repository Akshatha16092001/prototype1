using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private float speed = 10f;
    private float turnSpeed = 50f;
    private float horizontalInput;
    private float forwardInput;

    public bool canMove = true;

    [Header("Sound Settings")]
    public AudioClip engineSound;
    public AudioClip crashSound;
    private AudioSource playerAudio;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

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

    void FixedUpdate() // 🔥 use FixedUpdate for physics movement
    {
        if (!canMove)
        {
            if (playerAudio.isPlaying && playerAudio.clip == engineSound)
                playerAudio.Pause();
            return;
        }

        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        // Move forward/backward using Rigidbody
        Vector3 move = transform.forward * speed * forwardInput * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        // Rotate smoothly
        Quaternion turn = Quaternion.Euler(0, horizontalInput * turnSpeed * Time.fixedDeltaTime, 0);
        rb.MoveRotation(rb.rotation * turn);

        if (playerAudio.clip == engineSound)
            playerAudio.pitch = 1f + Mathf.Abs(forwardInput) * 0.5f;
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
