using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton reference

    [Header("References")]
    public Transform player;                 
    public TextMeshProUGUI scoreText;        
    public TextMeshProUGUI timerText;        
    public TextMeshProUGUI gameOverText;     
    public TextMeshProUGUI winText;          
    public GameObject restartButton;         
    public GameObject coinPrefab;            

    [Header("Settings")]
    public float finishZ = 385f;             

    [Header("Coin Spawn Settings")]
    public float spawnInterval = 2f;         
    public int coinsPerBatch = 3;            
    public float spawnDistanceAhead = 40f;   
    public float coinY = 1.5f;               
    public float coinXRange = 3f;            

    [Header("Difficulty Settings")]
    public float baseSpeedMultiplier = 1f;
    public float spawnIntervalMultiplier = 1f;

    private bool isGameOver = false;
    private bool hasWon = false;
    private float startZ;
    private float timer = 0f;
    private int distanceScore = 0;
    private int coinScore = 0;
    private float nextSpawnTime = 0f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
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

        // 🔥 Apply difficulty scaling
        int levelIndex = SceneManager.GetActiveScene().buildIndex;
        if (levelIndex == 1) // Level 2
        {
            baseSpeedMultiplier = 1.2f;
            spawnIntervalMultiplier = 0.9f;
        }
        else if (levelIndex == 2) // Level 3
        {
            baseSpeedMultiplier = 1.4f;
            spawnIntervalMultiplier = 0.8f;
        }

        spawnInterval *= spawnIntervalMultiplier;

        // ✅ Adjust player speed/turn parameters safely
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.baseSpeed *= baseSpeedMultiplier;
            pc.maxSpeed *= baseSpeedMultiplier;
            pc.turnSpeed *= baseSpeedMultiplier;
        }

        Debug.Log($"GameManager started at Level {levelIndex}. Difficulty applied.");
    }

    void Update()
    {
        if (player == null || isGameOver || hasWon) return;

        // Timer
        timer += Time.deltaTime;
        if (timerText != null)
            timerText.text = "Time: " + timer.ToString("F1") + "s";

        // Distance-based score
        float distance = player.position.z - startZ;
        distanceScore = Mathf.FloorToInt(distance);
        UpdateScoreUI();

        // Coin spawn
        if (Time.time >= nextSpawnTime && player.position.z < finishZ)
        {
            SpawnCoinsAhead();
            nextSpawnTime = Time.time + spawnInterval;
        }

        // Game Over check
        if (player.position.y < -5)
            GameOver();

        // Win check
        if (player.position.z >= finishZ)
            Win();
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

        Debug.Log($"Game over triggered at {timer:F1} seconds.");
    }

    void Win()
    {
        if (hasWon) return;
        hasWon = true;

        if (winText != null)
        {
            winText.enabled = true;
            winText.text = $"YOU WIN!\nTime: {timer:F1}s";
        }

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
            pc.canMove = false;

        Debug.Log($"YOU WIN! Finished in {timer:F1} seconds.");

        Invoke(nameof(LoadNextLevel), 2.5f);
    }

    void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextSceneIndex);
        else
            Debug.Log("🎉 All levels completed!");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
