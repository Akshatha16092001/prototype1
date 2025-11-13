using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.IO;

[Serializable]
public class SaveData
{
    public int totalCoins;
    public double nextRefillTimeUTC;
    public string lastLoginDate;
    public int currentDayIndex;
}

public class LobbyManager : MonoBehaviour
{
    [Header("Scene Settings")]
    public string gameSceneName = "Level1";

    [Header("UI References")]
    public TextMeshProUGUI coinText;
    public GameObject entryFeePopup;
    public TextMeshProUGUI popupMessage;
    public TextMeshProUGUI yesText;
    public TextMeshProUGUI noText;
    public TextMeshProUGUI addCoinsText;
    public TextMeshProUGUI dailyRewardText;
    public TextMeshProUGUI coinTimerText;

    [Header("Economy Settings")]
    public int entryFee = 100;

    [Header("Auto Coin Fill Settings")]
    public int autoCoinsPerHour = 100;
    private float refillInterval = 3600f; // 1 hour
    private double nextRefillTimeUTC;

    private int totalCoins = 0;

    // === DAILY LOGIN SYSTEM ===
    private int[] dailyRewards = { 100, 200, 300, 300, 300, 300 };
    private int currentDayIndex = 0;
    private DateTime lastLoginDate;
    private bool rewardClaimedToday = false;

    // === SAVE FILE PATH ===
    private string saveFilePath;

    void Start()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        Debug.Log($"Save file path: {saveFilePath}");

        LoadDataFromFile();

        UpdateCoinUI();
        if (entryFeePopup != null) entryFeePopup.SetActive(false);
        if (addCoinsText != null) addCoinsText.gameObject.SetActive(false);

        CheckDailyReward();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsTextClicked(yesText)) OnYesClicked();
            else if (IsTextClicked(noText)) OnNoClicked();
            else if (IsTextClicked(addCoinsText)) AddDummyCoins();
            else if (IsTextClicked(dailyRewardText)) ClaimDailyReward();
        }

        UpdateAutoCoinTimer();
    }

    // === DAILY REWARD ===
    void CheckDailyReward()
    {
        DateTime today = DateTime.Now.Date;

        if (lastLoginDate.Date < today)
        {
            rewardClaimedToday = false;
            int reward = dailyRewards[currentDayIndex];
            if (dailyRewardText != null)
            {
                dailyRewardText.gameObject.SetActive(true);
                dailyRewardText.text = $"Daily Reward: {reward} Coins (Tap to claim)";
            }
        }
        else
        {
            rewardClaimedToday = true;
            if (dailyRewardText != null)
            {
                dailyRewardText.gameObject.SetActive(true);
                dailyRewardText.text = $"Reward already claimed today!";
            }
        }
    }

    void ClaimDailyReward()
    {
        if (rewardClaimedToday) return;

        int reward = dailyRewards[currentDayIndex];
        totalCoins += reward;
        UpdateCoinUI();

        lastLoginDate = DateTime.Now.Date;
        currentDayIndex++;
        if (currentDayIndex >= dailyRewards.Length)
            currentDayIndex = dailyRewards.Length - 1;

        rewardClaimedToday = true;
        if (dailyRewardText != null)
            dailyRewardText.text = $"Claimed {reward} Coins!";

        SaveDataToFile();
    }

    // === PLAY LOGIC ===
    public void PlayGame()
    {
        if (totalCoins >= entryFee)
        {
            ShowPopup($"Pay {entryFee} coins to start the game?");
        }
        else
        {
            ShowPopup("Not enough coins! Please add more.");
            if (addCoinsText != null) addCoinsText.gameObject.SetActive(true);
        }
    }

    void ShowPopup(string message)
    {
        if (popupMessage != null) popupMessage.text = message;
        if (entryFeePopup != null) entryFeePopup.SetActive(true);
    }

    public void OnYesClicked()
    {
        if (totalCoins >= entryFee)
        {
            totalCoins -= entryFee;
            SaveDataToFile();
            UpdateCoinUI();

            if (entryFeePopup != null) entryFeePopup.SetActive(false);
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            ShowPopup("Not enough coins! Please add more.");
            if (addCoinsText != null) addCoinsText.gameObject.SetActive(true);
        }
    }

    public void OnNoClicked()
    {
        if (entryFeePopup != null) entryFeePopup.SetActive(false);
    }

    public void AddDummyCoins()
    {
        totalCoins += 1000;
        SaveDataToFile();
        UpdateCoinUI();

        if (addCoinsText != null) addCoinsText.gameObject.SetActive(false);
    }

    void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = "Coins: " + totalCoins;
    }

    // === AUTO COIN FILL ===
    void UpdateAutoCoinTimer()
    {
        double currentUTC = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        double remaining = nextRefillTimeUTC - currentUTC;

        if (remaining <= 0)
        {
            AddCoinsAutomatically();
            nextRefillTimeUTC = currentUTC + refillInterval;
            SaveDataToFile();
            remaining = refillInterval;
        }

        if (coinTimerText != null)
        {
            remaining = Math.Max(remaining, 0);
            int hours = (int)(remaining / 3600);
            int minutes = (int)((remaining % 3600) / 60);
            int seconds = (int)(remaining % 60);
            coinTimerText.text = $"Next coins in: {hours:D2}:{minutes:D2}:{seconds:D2}";
        }
    }

    void AddCoinsAutomatically()
    {
        totalCoins += autoCoinsPerHour;
        SaveDataToFile();
        UpdateCoinUI();
    }

    // === FILE SAVE/LOAD SYSTEM ===
    void SaveDataToFile()
    {
        SaveData data = new SaveData()
        {
            totalCoins = totalCoins,
            nextRefillTimeUTC = nextRefillTimeUTC,
            lastLoginDate = lastLoginDate.ToString("o"), // ISO 8601 format
            currentDayIndex = currentDayIndex
        };

        string json = JsonUtility.ToJson(data, true);

        try
        {
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"Save file written to: {saveFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($" Failed to save data: {e.Message}");
        }
    }

    void LoadDataFromFile()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                totalCoins = data.totalCoins;
                nextRefillTimeUTC = data.nextRefillTimeUTC;
                currentDayIndex = data.currentDayIndex;

                if (!string.IsNullOrEmpty(data.lastLoginDate))
                {
                    lastLoginDate = DateTime.Parse(data.lastLoginDate, null,
                        System.Globalization.DateTimeStyles.RoundtripKind);
                }
                else
                {
                    lastLoginDate = DateTime.MinValue;
                }

                Debug.Log("Save file loaded successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($" Failed to load data: {e.Message}");
                ResetDefaultData();
            }
        }
        else
        {
            Debug.Log("No save file found. Creating new data.");
            ResetDefaultData();
        }
    }

    void ResetDefaultData()
    {
        totalCoins = 500;
        nextRefillTimeUTC = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds + refillInterval;
        lastLoginDate = DateTime.MinValue;
        currentDayIndex = 0;
        SaveDataToFile();
    }

    bool IsTextClicked(TextMeshProUGUI tmp)
    {
        if (tmp == null) return false;
        RectTransform rect = tmp.GetComponent<RectTransform>();
        Vector2 localMousePos = rect.InverseTransformPoint(Input.mousePosition);
        return rect.rect.Contains(localMousePos);
    }
}
