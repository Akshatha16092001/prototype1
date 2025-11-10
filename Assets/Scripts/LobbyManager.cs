using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

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
    private float refillInterval = 3600f; // 1 hour in seconds
    private double nextRefillTimeUTC; // store as absolute UTC seconds

    private int totalCoins = 0;

    // === DAILY LOGIN SYSTEM ===
    private int[] dailyRewards = { 100, 200, 300, 300, 300, 300 };
    private int currentDayIndex = 0;
    private DateTime lastLoginDate;
    private bool rewardClaimedToday = false;

    void Start()
    {
        // ✅ Initialize coins only on first launch
        if (!PlayerPrefs.HasKey("HasStartedBefore"))
        {
            totalCoins = 500;
            PlayerPrefs.SetInt("TotalCoins", totalCoins);
            PlayerPrefs.SetInt("HasStartedBefore", 1);
            PlayerPrefs.Save();
        }
        else
        {
            totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        }
        UpdateCoinUI();

        if (entryFeePopup != null) entryFeePopup.SetActive(false);
        if (addCoinsText != null) addCoinsText.gameObject.SetActive(false);

        // Load daily reward data
        if (PlayerPrefs.HasKey("LastLoginDate"))
            lastLoginDate = DateTime.Parse(PlayerPrefs.GetString("LastLoginDate"));
        else
            lastLoginDate = DateTime.MinValue;

        currentDayIndex = PlayerPrefs.GetInt("LoginDayIndex", 0);

        CheckDailyReward();

        // === AUTO COIN FILL ===
        // Load next refill time from PlayerPrefs (stored as UTC seconds)
        if (PlayerPrefs.HasKey("NextCoinRefillTime"))
            nextRefillTimeUTC = double.Parse(PlayerPrefs.GetString("NextCoinRefillTime"));
        else
        {
            double currentUTC = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            nextRefillTimeUTC = currentUTC + refillInterval;
            PlayerPrefs.SetString("NextCoinRefillTime", nextRefillTimeUTC.ToString());
            PlayerPrefs.Save();
        }
    }

    void Update()
    {
        // Detect mouse clicks on TMP texts
        if (Input.GetMouseButtonDown(0))
        {
            if (IsTextClicked(yesText)) OnYesClicked();
            else if (IsTextClicked(noText)) OnNoClicked();
            else if (IsTextClicked(addCoinsText)) AddDummyCoins();
            else if (IsTextClicked(dailyRewardText)) ClaimDailyReward();
        }

        UpdateAutoCoinTimer();
    }

    // === DAILY LOGIN CHECK ===
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
                dailyRewardText.text = $" Daily Reward: {reward} Coins (Tap to claim)";
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

        PlayerPrefs.SetInt("TotalCoins", totalCoins);

        lastLoginDate = DateTime.Now.Date;
        PlayerPrefs.SetString("LastLoginDate", lastLoginDate.ToString());

        currentDayIndex++;
        if (currentDayIndex >= dailyRewards.Length) currentDayIndex = dailyRewards.Length - 1;
        PlayerPrefs.SetInt("LoginDayIndex", currentDayIndex);
        PlayerPrefs.Save();

        rewardClaimedToday = true;
        if (dailyRewardText != null)
            dailyRewardText.text = $"Claimed {reward} Coins!";
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
            PlayerPrefs.SetInt("TotalCoins", totalCoins);
            PlayerPrefs.Save();
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
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.Save();
        UpdateCoinUI();

        if (addCoinsText != null) addCoinsText.gameObject.SetActive(false);
    }

    void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = "Coins: " + totalCoins;
    }

    // === AUTO COIN FILL METHODS ===
    void UpdateAutoCoinTimer()
    {
        double currentUTC = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        double remaining = nextRefillTimeUTC - currentUTC;

        if (remaining <= 0)
        {
            AddCoinsAutomatically();
            nextRefillTimeUTC = currentUTC + refillInterval;
            PlayerPrefs.SetString("NextCoinRefillTime", nextRefillTimeUTC.ToString());
            PlayerPrefs.Save();
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
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        UpdateCoinUI();
    }

    bool IsTextClicked(TextMeshProUGUI tmp)
    {
        if (tmp == null) return false;

        RectTransform rect = tmp.GetComponent<RectTransform>();
        Vector2 localMousePos = rect.InverseTransformPoint(Input.mousePosition);
        return rect.rect.Contains(localMousePos);
    }
}
