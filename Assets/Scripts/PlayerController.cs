using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float speed = 5f;
    private float turnSpeed = 25f;
    private float horizontalInput;
    private float forwardInput;

    public bool canMove = true; // Controlled by GameManager (stops after win or game over)

    [Header("Sound Settings")]
    public AudioClip engineSound;  // Looping engine clip
    public AudioClip crashSound;   // Crash clip
    private AudioSource playerAudio;

    void Start()
    {
        // Get or add AudioSource
        playerAudio = GetComponent<AudioSource>();
        if (playerAudio == null)
            playerAudio = gameObject.AddComponent<AudioSource>();

        // Start looping engine sound
        if (engineSound != null)
        {
            playerAudio.clip = engineSound;
            playerAudio.loop = true;
            playerAudio.Play();
        }
    }

    void Update()
    {
        if (!canMove)
        {
            // Stop engine loop when player can’t move
            if (playerAudio.isPlaying && playerAudio.clip == engineSound)
                playerAudio.Pause();
            return;
        }

        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        // Move and rotate
        transform.Translate(Vector3.forward * Time.deltaTime * speed * forwardInput);
        transform.Rotate(Vector3.up, turnSpeed * horizontalInput * Time.deltaTime);

        // Adjust engine pitch (optional realism)
        if (playerAudio.clip == engineSound)
            playerAudio.pitch = 1f + Mathf.Abs(forwardInput) * 0.5f;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Handle collision with obstacles
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Player hit an obstacle! Game Over triggered.");

            canMove = false;

            // Stop engine loop
            if (playerAudio.clip == engineSound)
                playerAudio.Stop();

            // Play crash sound once
            if (crashSound != null)
                playerAudio.PlayOneShot(crashSound, 1.0f);

            // Notify GameManager
            FindObjectOfType<GameManager>().SendMessage("GameOver");
        }
    }
}
