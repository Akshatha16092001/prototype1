using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class LobbyManager : MonoBehaviour
{
    [Header("Scene Settings")]
    public string gameSceneName = "Level1"; // Your gameplay scene name

    [Header("UI References")]
    public TextMeshProUGUI coinText;        // Top-left coin display
    public GameObject entryFeePopup;        // Popup panel (disabled by default)
    public TextMeshProUGUI popupMessage;    // Text inside popup
    public TextMeshProUGUI yesText;         // TextMeshPro for "Yes"
    public TextMeshProUGUI noText;          // TextMeshPro for "No"
    public TextMeshProUGUI addCoinsText;    // TextMeshPro for "+" button
    public TextMeshProUGUI autoCoinTimerText; // Timer for auto coin fill

    [Header("Economy Settings")]
    public int entryFee = 100;
    public int startingCoins = 500;
    public int autoCoinsPerHour = 100;

    private int totalCoins = 0;
    private DateTime lastCoinTime;
    private TimeSpan timeUntilNextCoin;

    void Start()
    {
        LoadCoins();
        UpdateCoinUI();

        if (entryFeePopup != null)
            entryFeePopup.SetActive(false);

        if (addCoinsText != null)
            addCoinsText.gameObject.SetActive(false);

        // Load last coin time
        if (PlayerPrefs.HasKey("LastCoinTime"))
            DateTime.TryParse(PlayerPrefs.GetString("LastCoinTime"), out lastCoinTime);
        else
        {
            lastCoinTime = DateTime.Now;
            PlayerPrefs.SetString("LastCoinTime", lastCoinTime.ToString());
            PlayerPrefs.Save();
        }
    }

    void Update()
    {
        // Detect mouse click on TMP "buttons"
        if (Input.GetMouseButtonDown(0))
        {
            if (IsTextClicked(yesText))
                OnYesClicked();
            else if (IsTextClicked(noText))
                OnNoClicked();
            else if (IsTextClicked(addCoinsText))
                AddDummyCoins();
        }

        UpdateAutoCoinTimerUI();
    }

    void LoadCoins()
    {
        if (PlayerPrefs.HasKey("TotalCoins"))
        {
            totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        }
        else
        {
            totalCoins = startingCoins;
            PlayerPrefs.SetInt("TotalCoins", totalCoins);
            PlayerPrefs.Save();
        }
    }

    // Called when Play button pressed
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
        totalCoins += 1000; // Add dummy coins
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

    // --- Auto Coin Fill Logic ---
    void UpdateAutoCoinTimerUI()
    {
        if (autoCoinTimerText == null) return;

        TimeSpan timePassed = DateTime.Now - lastCoinTime;
        timeUntilNextCoin = TimeSpan.FromHours(1) - timePassed;

        if (timeUntilNextCoin.TotalSeconds <= 0)
        {
            // Add coins automatically
            totalCoins += autoCoinsPerHour;
            PlayerPrefs.SetInt("TotalCoins", totalCoins);

            // Reset last coin time
            lastCoinTime = DateTime.Now;
            PlayerPrefs.SetString("LastCoinTime", lastCoinTime.ToString());
            PlayerPrefs.Save();

            UpdateCoinUI();

            timeUntilNextCoin = TimeSpan.FromHours(1);
        }

        autoCoinTimerText.text = $"Next +{autoCoinsPerHour} Coins in {timeUntilNextCoin.Hours:D2}:{timeUntilNextCoin.Minutes:D2}:{timeUntilNextCoin.Seconds:D2}";
    }
}
