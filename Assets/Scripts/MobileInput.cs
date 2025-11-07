using UnityEngine;

public class MobileInput : MonoBehaviour
{
    // Public static variables read by PlayerController
    public static float horizontal = 0f;
    public static float vertical = 0f;

    // Singleton pattern to ensure only one instance persists across scenes
    private static MobileInput instance;

    void Awake()
    {
        // If another MobileInput exists, destroy it
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Persist across all levels
    }

    // Left / Right movement
    public void MoveLeftDown()
    { 
        horizontal = -1f; 
        Debug.Log("Left Down"); 
    }

    public void MoveLeftUp()
    { 
        horizontal = 0f; 
        Debug.Log("Left Up"); 
    }

    public void MoveRightDown()
    { 
        horizontal = 1f; 
        Debug.Log("Right Down"); 
    }

    public void MoveRightUp()
    { 
        horizontal = 0f; 
        Debug.Log("Right Up"); 
    }

    // Accelerate / Brake
    public void AccelerateDown()
    { 
        vertical = 1f; 
        Debug.Log("Accelerate Down"); 
    }

    public void AccelerateUp()
    { 
        vertical = 0f; 
        Debug.Log("Accelerate Up"); 
    }

    public void BrakeDown()
    { 
        vertical = -1f; 
        Debug.Log("Brake Down"); 
    }

    public void BrakeUp()
    { 
        vertical = 0f; 
        Debug.Log("Brake Up"); 
    }

    // Remove OnDisable entirely — do not reset values automatically
}
