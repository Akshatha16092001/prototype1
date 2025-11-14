using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

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
    public int maxAttempts = 3;
    [HideInInspector] public int remainingAttempts;

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
    private bool isFalling = false;
    private string saveFilePath;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        remainingAttempts = maxAttempts;

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

        int levelIndex = SceneManager.GetActiveScene().buildIndex;
        if (levelIndex == 1)
        {
            baseSpeedMultiplier = 1.2f;
            spawnIntervalMultiplier = 0.9f;
        }
        else if (levelIndex == 2)
        {
            baseSpeedMultiplier = 1.4f;
            spawnIntervalMultiplier = 0.8f;
        }

        spawnInterval *= spawnIntervalMultiplier;

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.baseSpeed *= baseSpeedMultiplier;
            pc.maxSpeed *= baseSpeedMultiplier;
            pc.turnSpeed *= baseSpeedMultiplier;
        }

        saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        Debug.Log("GameManager started. Save path: " + saveFilePath);
    }

    void Update()
    {
        if (player == null || isGameOver || hasWon) return;

        timer += Time.deltaTime;
        if (timerText != null)
            timerText.text = "Time: " + timer.ToString("F1") + "s";

        float distance = player.position.z - startZ;
        distanceScore = Mathf.FloorToInt(distance);
        UpdateScoreUI();

        if (Time.time >= nextSpawnTime && player.position.z < finishZ)
        {
            SpawnCoinsAhead();
            nextSpawnTime = Time.time + spawnInterval;
        }

        if (player.position.y < -5 && !isFalling)
        {
            isFalling = true;
            HandleAttempt();
        }
        else if (player.position.y >= -5)
        {
            isFalling = false;
        }

        if (player.position.z >= finishZ)
            Win();
    }

    void SpawnCoinsAhead()
    {
        if (coinPrefab == null || player == null) return;

        for (int i = 0; i < coinsPerBatch; i++)
        {
            float randomX = UnityEngine.Random.Range(-coinXRange, coinXRange);
            float randomZ = player.position.z + UnityEngine.Random.Range(10f, spawnDistanceAhead);
            Vector3 spawnPos = new Vector3(randomX, coinY, randomZ);
            Instantiate(coinPrefab, spawnPos, Quaternion.identity);
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + (distanceScore + coinScore);
    }

    public void AddCoinScore(int amount)
    {
        coinScore += amount;
        UpdateScoreUI();
    }

    void HandleAttempt()
    {
        if (isGameOver) return;

        remainingAttempts--;
        Debug.Log("Player fell! Remaining Attempts: " + remainingAttempts);

        if (remainingAttempts > 0)
        {
            StartCoroutine(RespawnRoutine());
        }
        else
        {
            GameOver();
        }
    }

    IEnumerator RespawnRoutine()
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
            pc.canMove = false;

        yield return new WaitForSeconds(1.0f);
        RespawnPlayer();
    }

    void RespawnPlayer()
    {
        if (isGameOver) return;

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
            pc.ResetToStart();

        Debug.Log("Player respawned successfully.");
    }

    public void GameOver()
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

        int reward = 200;
        AddRewardToSaveFile(reward);

        if (winText != null)
        {
            winText.enabled = true;
            winText.text = "YOU WIN!\nTime: " + timer.ToString("F1") + "s\n+" + reward + " Coins!";
        }

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
            pc.canMove = false;

        Debug.Log("YOU WIN! Finished in " + timer.ToString("F1") + " seconds. +" + reward + " coins added.");

        Invoke(nameof(LoadNextLevel), 2.5f);
    }

    void AddRewardToSaveFile(int reward)
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                data.totalCoins += reward;

                string updatedJson = JsonUtility.ToJson(data, true);
                File.WriteAllText(saveFilePath, updatedJson);
                Debug.Log("Added " + reward + " coins to save file.");

                PlayerPrefs.SetInt("CoinsEarned", reward);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogWarning("Save file not found. Reward not saved.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error updating save file: " + e.Message);
        }
    }

    void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextSceneIndex);
        else
            Debug.Log("All levels completed!");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
