using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;
using System.IO;
using DG.Tweening;

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
    private float refillInterval = 3600f;

    private int totalCoins = 0;
    private int[] dailyRewards = { 100, 200, 300, 300, 300, 300 };
    private int currentDayIndex = 0;
    private DateTime nextRefillTimeUTC;
    private DateTime lastLoginDate;
    private bool rewardClaimedToday = false;
    private bool hasPaidEntry = false;
    private string saveFilePath;

    // Tween references
    private Tween popupTween;
    private Tween coinTween;
    private Tween dailyTween;

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

    void OnDestroy()//destroy
    {
        popupTween?.Kill();
        coinTween?.Kill();
        dailyTween?.Kill();
    }

    void Update()
    {
        UpdateRefillTimer();

        if (Input.GetMouseButtonDown(0))
        {
            if (IsTextClicked(dailyRewardText)) ClaimDailyReward();
            if (IsTextClicked(addCoinsText)) AddDummyCoins();
        }
    }

    // ===================== DAILY REWARD =====================
    void CheckDailyReward()
    {
        DateTime today = DateTime.Now.Date;

        if (lastLoginDate.Date < today)
        {
            rewardClaimedToday = false;
            int reward = dailyRewards[currentDayIndex];
            dailyRewardText.text = $"Daily Reward: {reward} Coins (Tap to claim)";
            AnimateDailyReward();
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
        if (currentDayIndex >= dailyRewards.Length) currentDayIndex = 0;

        dailyRewardText.text = $"Claimed {reward} Coins!";
        UpdateCoinUI();
        SaveDataFile();

        Debug.Log($"Daily reward claimed: {reward} coins.");

        AnimateCoinText();
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
            AnimateAddCoins();
        }

        entryFeePopup.SetActive(true);
        AnimatePopupOpen();
    }

    public void OnYesClicked()
    {
        if (hasPaidEntry) return;

        if (totalCoins < entryFee)
        {
            popupMessage.text = "Not enough coins!";
            return;
        }

        totalCoins -= entryFee;
        hasPaidEntry = true;

        UpdateCoinUI();
        SaveDataFile();

        AnimatePopupClose(() =>
        {
            SceneManager.LoadScene(gameSceneName);
        });
    }

    public void OnNoClicked()
    {
        hasPaidEntry = false;
        AnimatePopupClose(null);
    }

    // ===================== COINS =====================
    void AddDummyCoins()
    {
        totalCoins += 1000;
        addCoinsText.gameObject.SetActive(false);
        UpdateCoinUI();
        SaveDataFile();
        AnimateCoinText();
    }

    void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = $"Coins: {totalCoins}";
    }

    // ===================== AUTO REFILL =====================
    void UpdateRefillTimer()
    {
        DateTime now = DateTime.Now;
        DateTime nextLocal = nextRefillTimeUTC.ToLocalTime();

        if (nextRefillTimeUTC == default || now >= nextLocal)
        {
            totalCoins += autoCoinsPerHour;
            nextRefillTimeUTC = DateTime.UtcNow.AddHours(1);
            SaveDataFile();
            UpdateCoinUI();
            AnimateCoinText();
            nextLocal = nextRefillTimeUTC.ToLocalTime();
        }

        TimeSpan remaining = nextLocal - now;
        if (remaining.TotalSeconds < 0) remaining = TimeSpan.Zero;

        coinTimerText.text = $"Next refill in: {remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
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
        }
        catch
        {
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

    // ===================== DOTWEEN ANIMATIONS =====================
    void AnimatePopupOpen()
    {
        RectTransform rt = entryFeePopup.GetComponent<RectTransform>();
        if (rt == null) return;

        rt.localScale = Vector3.zero;
        popupTween?.Kill();
        popupTween = rt.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
    }

    void AnimatePopupClose(Action onComplete)
    {
        RectTransform rt = entryFeePopup.GetComponent<RectTransform>();
        if (rt == null) { onComplete?.Invoke(); return; }

        popupTween?.Kill();
        popupTween = rt.DOScale(0f, 0.25f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                entryFeePopup.SetActive(false);
                onComplete?.Invoke();
            });
    }

    void AnimateCoinText()
    {
        if (coinText == null) return;
        RectTransform rt = coinText.rectTransform;

        coinTween?.Kill();
        coinTween = rt.DOPunchScale(Vector3.one * 0.25f, 0.3f, 10, 1f);
    }

    void AnimateDailyReward()
    {
        if (dailyRewardText == null) return;
        RectTransform rt = dailyRewardText.rectTransform;

        dailyTween?.Kill();
        dailyTween = rt.DOScale(1.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }

    void AnimateAddCoins()//Animations
    {
        if (addCoinsText == null) return;

        addCoinsText.DOFade(1f, 0.3f).SetLoops(6, LoopType.Yoyo);
    }

    bool IsTextClicked(TextMeshProUGUI tmp)
    {
        if (tmp == null) return false;
        RectTransform rect = tmp.GetComponent<RectTransform>();
        Vector2 localPos = rect.InverseTransformPoint(Input.mousePosition);
        return rect.rect.Contains(localPos);
    }
}
