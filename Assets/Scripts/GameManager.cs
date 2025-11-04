using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public Transform player;                 // Drag your Player (car)
    public TextMeshProUGUI scoreText;        // Drag your ScoreText (TMP)
    public TextMeshProUGUI timerText;        // Drag your TimerText (TMP)
    public TextMeshProUGUI gameOverText;     // Drag your GameOverText (TMP)
    public TextMeshProUGUI winText;          // Drag your WinText (TMP)
    public GameObject restartButton;         // Drag your RestartButton (UI Button)

    [Header("Settings")]
    public float finishZ = 385f;             // Adjust to match end of track

    private bool isGameOver = false;
    private bool hasWon = false;
    private float startZ;
    private float timer = 0f;                // Timer variable

    void Start()
    {
        // Initialize UI
        if (scoreText != null) scoreText.text = "Score: 0";
        if (timerText != null) timerText.text = "Time: 0.0s";
        if (gameOverText != null) gameOverText.text = "";
        if (winText != null)
        {
            winText.text = "";
            winText.enabled = true;
        }

        if (restartButton != null)
            restartButton.SetActive(false);

        if (player != null)
            startZ = player.position.z;

        timer = 0f;

        Debug.Log("GameManager started. Timer initialized.");
    }

    void Update()
    {
        if (player == null) return;
        if (isGameOver || hasWon) return;

        // ⏱️ Update timer
        timer += Time.deltaTime;
        if (timerText != null)
            timerText.text = "Time: " + timer.ToString("F1") + "s";

        // 📈 Update score based on distance moved
        float distance = player.position.z - startZ;
        if (scoreText != null)
            scoreText.text = "Score: " + Mathf.FloorToInt(distance).ToString();

        // Game Over check (player falls off)
        if (player.position.y < -5)
        {
            GameOver();
        }

        // 🎯 Win check (player crosses finish line)
        if (player.position.z >= finishZ)
        {
            Win();
        }
    }

    void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (gameOverText != null)
        {
            gameOverText.enabled = true;
            gameOverText.text = "GAME OVER!";
        }

        if (restartButton != null)
            restartButton.SetActive(true);

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
            pc.canMove = false;

        Debug.Log("Game over triggered at " + timer.ToString("F1") + " seconds.");
    }

    void Win()
    {
        if (hasWon) return;
        hasWon = true;

        if (winText != null)
        {
            winText.enabled = true;
            winText.text = "YOU WIN!\nTime: " + timer.ToString("F1") + "s";
        }

        if (restartButton != null)
            restartButton.SetActive(true);

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
            pc.canMove = false;

        Debug.Log("YOU WIN! Finished in " + timer.ToString("F1") + " seconds.");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
