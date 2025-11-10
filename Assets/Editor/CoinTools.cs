using UnityEditor;
using UnityEngine;

public class CoinTools
{
    [MenuItem("Tools/Reset Coins")]
    public static void ResetAllCoins()
    {
        PlayerPrefs.DeleteKey("TotalCoins");
        PlayerPrefs.Save();
        Debug.Log("✅ All coins reset successfully!");
    }
}
