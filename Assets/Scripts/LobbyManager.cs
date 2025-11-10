using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System; // For DateTime

public class LobbyManager : MonoBehaviour
{
    [Header("Scene Settings")]
    public string gameSceneName = "Level1"; // Your gameplay scene name

    [Header("UI References")]
    public TextMeshProUGUI coinText;        
    public GameObject entryFeePopup;        
    public TextMeshProUGUI popupMessage;    
    public TextMeshProUGUI yesText;         
    public TextMeshProUGUI noText;          
    public TextMeshProUGUI addCoinsText;    
    public TextMeshProUGUI dailyRewardText; // Daily reward TMP text

    [Header("Economy Settings")]
    public int entryFee = 100;

    private int totalCoins = 0;

    // === DAILY LOGIN SYSTEM ===
    private int[] dailyRewards = { 100, 200, 300, 300, 300, 300 };
    private int currentDayIndex = 0;
    private DateTime lastLoginDate;
    private bool rewardClaimedToday = false;

    void Start()
    {
        // Load coins
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        if (totalCoins == 0)
        {
            totalCoins = 500;
            PlayerPrefs.SetInt("TotalCoins", totalCoins);
            PlayerPrefs.Save();
        }

        UpdateCoinUI();

        if (entryFeePopup != null)
            entryFeePopup.SetActive(false);

        if (addCoinsText != null)
            addCoinsText.gameObject.SetActive(false);

        // Load daily reward data
        if (PlayerPrefs.HasKey("LastLoginDate"))
            lastLoginDate = DateTime.Parse(PlayerPrefs.GetString("LastLoginDate"));
        else
            lastLoginDate = DateTime.MinValue;

        currentDayIndex = PlayerPrefs.GetInt("LoginDayIndex", 0);

        CheckDailyReward();
    }

    void Update()
    {
        // Detect mouse clicks on TMP texts
        if (Input.GetMouseButtonDown(0))
        {
            if (IsTextClicked(yesText))
                OnYesClicked();
            else if (IsTextClicked(noText))
                OnNoClicked();
            else if (IsTextClicked(addCoinsText))
                AddDummyCoins();
            else if (IsTextClicked(dailyRewardText))
                ClaimDailyReward();
        }
    }

    // === DAILY LOGIN CHECK ===
    void CheckDailyReward()
    {
        DateTime today = DateTime.Now.Date;

        if (lastLoginDate.Date < today)
        {
            // New day — reward available
            rewardClaimedToday = false;

            int reward = dailyRewards[currentDayIndex];
            if (dailyRewardText != null)
            {
                dailyRewardText.gameObject.SetActive(true);
                dailyRewardText.text = $"🎁 Daily Reward: {reward} Coins (Tap to claim)";
            }
        }
        else
        {
            // Already claimed today
            rewardClaimedToday = true;
            if (dailyRewardText != null)
            {
                dailyRewardText.gameObject.SetActive(true);
                dailyRewardText.text = $"✅ Reward already claimed today!";
            }
        }
    }

    // === CLAIM DAILY REWARD ===
    void ClaimDailyReward()
    {
        if (rewardClaimedToday) return;

        int reward = dailyRewards[currentDayIndex];
        totalCoins += reward;
        UpdateCoinUI();

        // Save new coin total
        PlayerPrefs.SetInt("TotalCoins", totalCoins);

        // Update login data
        lastLoginDate = DateTime.Now.Date;
        PlayerPrefs.SetString("LastLoginDate", lastLoginDate.ToString());

        currentDayIndex++;
        if (currentDayIndex >= dailyRewards.Length)
            currentDayIndex = dailyRewards.Length - 1; // stay at last reward
        PlayerPrefs.SetInt("LoginDayIndex", currentDayIndex);
        PlayerPrefs.Save();

        rewardClaimedToday = true;
        if (dailyRewardText != null)
            dailyRewardText.text = $"🎉 Claimed {reward} Coins!";
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
            if (addCoinsText != null)
                addCoinsText.gameObject.SetActive(true);
        }
    }

    void ShowPopup(string message)
    {
        if (popupMessage != null)
            popupMessage.text = message;

        if (entryFeePopup != null)
            entryFeePopup.SetActive(true);
    }

    public void OnYesClicked()
    {
        if (totalCoins >= entryFee)
        {
            totalCoins -= entryFee;
            PlayerPrefs.SetInt("TotalCoins", totalCoins);
            PlayerPrefs.Save();
            UpdateCoinUI();

            if (entryFeePopup != null)
                entryFeePopup.SetActive(false);

            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            ShowPopup("Not enough coins! Please add more.");
            if (addCoinsText != null)
                addCoinsText.gameObject.SetActive(true);
        }
    }

    public void OnNoClicked()
    {
        if (entryFeePopup != null)
            entryFeePopup.SetActive(false);
    }

    public void AddDummyCoins()
    {
        totalCoins += 1000;
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.Save();
        UpdateCoinUI();

        if (addCoinsText != null)
            addCoinsText.gameObject.SetActive(false);
    }

    void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = "Coins: " + totalCoins;
    }

    bool IsTextClicked(TextMeshProUGUI tmp)
    {
        if (tmp == null) return false;

        RectTransform rect = tmp.GetComponent<RectTransform>();
        Vector2 localMousePos = rect.InverseTransformPoint(Input.mousePosition);
        return rect.rect.Contains(localMousePos);
    }
}
