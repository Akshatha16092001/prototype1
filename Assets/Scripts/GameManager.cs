using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton reference for easy access from Coin script

    [Header("References")]
    public Transform player;                 // Drag your Player (car)
    public TextMeshProUGUI scoreText;        // Drag your ScoreText (TMP)
    public TextMeshProUGUI timerText;        // Drag your TimerText (TMP)
    public TextMeshProUGUI gameOverText;     // Drag your GameOverText (TMP)
    public TextMeshProUGUI winText;          // Drag your WinText (TMP)
    public GameObject restartButton;         // Drag your RestartButton (UI Button)
    public GameObject coinPrefab;            // 🪙 Drag your coin prefab here in the Inspector

    [Header("Settings")]
    public float finishZ = 385f;             // Adjust to match end of track

    [Header("Coin Spawn Settings")]
    public float spawnInterval = 2f;         // Seconds between coin spawns
    public int coinsPerBatch = 3;            // Coins per batch
    public float spawnDistanceAhead = 40f;   // Distance ahead of player
    public float coinY = 1.5f;               // Coin height above road
    public float coinXRange = 3f;            // Left-right range for coins

    private bool isGameOver = false;
    private bool hasWon = false;
    private float startZ;
    private float timer = 0f;                // Timer variable
    private int distanceScore = 0;           // Based on travel
    private int coinScore = 0;               // Based on coins collected
    private float nextSpawnTime = 0f;        // Timer to control coin spawn rate

    void Awake()
    {
        // Ensure singleton
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

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
        nextSpawnTime = Time.time + spawnInterval;

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
        distanceScore = Mathf.FloorToInt(distance);
        UpdateScoreUI();

        // 🪙 Spawn coins automatically
        if (Time.time >= nextSpawnTime && player.position.z < finishZ)
        {
            SpawnCoinsAhead();
            nextSpawnTime = Time.time + spawnInterval;
        }

        // 💀 Game Over check
        if (player.position.y < -5)
        {
            GameOver();
        }

        // 🎯 Win check
        if (player.position.z >= finishZ)
        {
            Win();
        }
    }

    void SpawnCoinsAhead()
    {
        if (coinPrefab == null || player == null) return;

        for (int i = 0; i < coinsPerBatch; i++)
        {
            float randomX = Random.Range(-coinXRange, coinXRange);
            float randomZ = player.position.z + Random.Range(10f, spawnDistanceAhead);
            Vector3 spawnPos = new Vector3(randomX, coinY, randomZ);

            Instantiate(coinPrefab, spawnPos, Quaternion.identity);
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + (distanceScore + coinScore).ToString();
    }

    public void AddCoinScore(int amount)
    {
        coinScore += amount;
        UpdateScoreUI();
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

        // ✅ Automatically load next level if it exists
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Invoke("LoadNextLevel", 2f); // Wait 2 seconds before next level
        }
    }

    void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(nextSceneIndex);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
