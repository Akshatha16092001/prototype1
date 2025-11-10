using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

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

    [Header("Economy Settings")]
    public int entryFee = 100;

    private int totalCoins = 0;

    void Start()
    {
        // Load coins from PlayerPrefs (don't reset if zero)
        if (PlayerPrefs.HasKey("TotalCoins"))
        {
            totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        }
        else
        {
            // First-time players start with 500 coins
            totalCoins = 500;
            PlayerPrefs.SetInt("TotalCoins", totalCoins);
            PlayerPrefs.Save();
        }

        UpdateCoinUI();

        if (entryFeePopup != null)
            entryFeePopup.SetActive(false);

        if (addCoinsText != null)
            addCoinsText.gameObject.SetActive(false); // hide "+" at start
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

            // Load next scene
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
}
