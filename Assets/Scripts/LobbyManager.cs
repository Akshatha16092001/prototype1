using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;
using System.IO;

[Serializable]
public class SaveData
{
    public int totalCoins = 500;
    public string lastLoginDate = "";
    public int currentDayIndex = 0;
    public string nextRefillTimeUTC = "";
}

public class LobbyManager : MonoBehaviour
{
    [Header("Scene Settings")]
    public string gameSceneName = "Level1";

    [Header("UI References")]
    public TextMeshProUGUI coinText;
    public GameObject entryFeePopup;
    public TextMeshProUGUI popupMessage;
    public Button yesButton;
    public Button noButton;
    public TextMeshProUGUI addCoinsText;
    public TextMeshProUGUI dailyRewardText;
    public TextMeshProUGUI coinTimerText;
    public Button playButton;

    [Header("Economy Settings")]
    public int entryFee = 100;

    [Header("Auto Refill Settings")]
    public int autoCoinsPerHour = 100;
    private float refillInterval = 3600f; // 1 hour in seconds

    private int totalCoins = 0;
    private int[] dailyRewards = { 100, 200, 300, 300, 300, 300 };
    private int currentDayIndex = 0;
    private DateTime nextRefillTimeUTC;
    private DateTime lastLoginDate;
    private bool rewardClaimedToday = false;
    private bool hasPaidEntry = false;
    private string saveFilePath;

    void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        Debug.Log("Save file path: " + saveFilePath);
    }

    void Start()
    {
        LoadData();
        UpdateCoinUI();

        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);
        playButton.onClick.AddListener(PlayGame);

        entryFeePopup.SetActive(false);
        addCoinsText.gameObject.SetActive(false);

        CheckDailyReward();
        SaveDataFile();
    }

    void Update()
    {
        UpdateRefillTimer();

        if (Input.GetMouseButtonDown(0))
        {
            if (IsTextClicked(dailyRewardText))
                ClaimDailyReward();

            if (IsTextClicked(addCoinsText))
                AddDummyCoins();
        }
    }

    // ===================== DAILY REWARD =====================
    void CheckDailyReward()
    {
        DateTime today = DateTime.Now.Date;

        // Reward can only be claimed if a new real day has started
        if (lastLoginDate.Date < today)
        {
            rewardClaimedToday = false;
            int reward = dailyRewards[currentDayIndex];
            dailyRewardText.text = $"Daily Reward: {reward} Coins (Tap to claim)";
        }
        else
        {
            rewardClaimedToday = true;
            dailyRewardText.text = "Reward already claimed today!";
        }

        dailyRewardText.gameObject.SetActive(true);
    }

    void ClaimDailyReward()
    {
        if (rewardClaimedToday) return;

        int reward = dailyRewards[currentDayIndex];
        totalCoins += reward;
        rewardClaimedToday = true;
        lastLoginDate = DateTime.Now.Date;

        currentDayIndex++;
        if (currentDayIndex >= dailyRewards.Length)
            currentDayIndex = 0;

        dailyRewardText.text = $"Claimed {reward} Coins!";
        UpdateCoinUI();
        SaveDataFile();

        Debug.Log($"Daily reward claimed: {reward} coins.");
    }

    // ===================== PLAY BUTTON =====================
    public void PlayGame()
    {
        if (entryFeePopup.activeSelf) return;

        if (totalCoins >= entryFee)
        {
            popupMessage.text = $"Pay {entryFee} coins to start the game?";
        }
        else
        {
            popupMessage.text = "Not enough coins!";
            addCoinsText.gameObject.SetActive(true);
        }

        entryFeePopup.SetActive(true);
    }

    public void OnYesClicked()
    {
        if (hasPaidEntry) return;

        if (totalCoins < entryFee)
        {
            popupMessage.text = "Not enough coins!";
            return;
        }

        //  Deduct exactly 100 coins
        totalCoins -= entryFee;
        hasPaidEntry = true;

        UpdateCoinUI();
        SaveDataFile();

        entryFeePopup.SetActive(false);
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnNoClicked()
    {
        entryFeePopup.SetActive(false);
        hasPaidEntry = false;
    }

    // ===================== COINS =====================
    void AddDummyCoins()
    {
        totalCoins += 1000;
        addCoinsText.gameObject.SetActive(false);
        UpdateCoinUI();
        SaveDataFile();
    }

    void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = $"Coins: {totalCoins}";
    }

    // ===================== AUTO REFILL (Fixed Local Time Countdown) =====================
    void UpdateRefillTimer()
    {
        DateTime now = DateTime.Now; // local time
        DateTime nextRefillLocal = nextRefillTimeUTC.ToLocalTime(); // convert UTC to local for display

        // Check refill time
        if (nextRefillTimeUTC == default || now >= nextRefillLocal)
        {
            totalCoins += autoCoinsPerHour;
            nextRefillTimeUTC = DateTime.UtcNow.AddHours(1); // store in UTC for consistency
            SaveDataFile();
            UpdateCoinUI();
            nextRefillLocal = nextRefillTimeUTC.ToLocalTime();
        }

        // Calculate remaining countdown
        TimeSpan remaining = nextRefillLocal - now;
        if (remaining.TotalSeconds < 0)
            remaining = TimeSpan.Zero;

        if (coinTimerText != null)
        {
            int h = remaining.Hours;
            int m = remaining.Minutes;
            int s = remaining.Seconds;
            coinTimerText.text = $"Next refill in: {h:D2}:{m:D2}:{s:D2}";
        }
    }

    // ===================== SAVE / LOAD =====================
    void SaveDataFile()
    {
        SaveData data = new SaveData
        {
            totalCoins = totalCoins,
            lastLoginDate = lastLoginDate.ToString("yyyy-MM-dd"),
            currentDayIndex = currentDayIndex,
            nextRefillTimeUTC = nextRefillTimeUTC.ToString("o")
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Save file updated: " + json);
    }

    void LoadData()
    {
        if (!File.Exists(saveFilePath))
        {
            ResetDefaults();
            return;
        }

        try
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            totalCoins = data.totalCoins;
            currentDayIndex = data.currentDayIndex;
            DateTime.TryParse(data.lastLoginDate, out lastLoginDate);
            DateTime.TryParse(data.nextRefillTimeUTC, out nextRefillTimeUTC);

            if (nextRefillTimeUTC == default)
                nextRefillTimeUTC = DateTime.UtcNow.AddSeconds(refillInterval);

            Debug.Log("Save file loaded successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to load save data: " + ex.Message);
            ResetDefaults();
        }
    }

    void ResetDefaults()
    {
        totalCoins = 500;
        currentDayIndex = 0;
        lastLoginDate = DateTime.MinValue;
        nextRefillTimeUTC = DateTime.UtcNow.AddHours(1);
        SaveDataFile();
    }
    public void QuitGame()
{
    Application.Quit();
}


    // ===================== HELPER =====================
    bool IsTextClicked(TextMeshProUGUI tmp)
    {
        if (tmp == null) return false;
        RectTransform rect = tmp.GetComponent<RectTransform>();
        Vector2 localPos = rect.InverseTransformPoint(Input.mousePosition);
        return rect.rect.Contains(localPos);
    }
}
