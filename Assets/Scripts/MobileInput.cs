using UnityEngine;

public class MobileInput : MonoBehaviour
{
    // Public static so PlayerController can read them easily
    public static float horizontal = 0f;
    public static float vertical = 0f;

    // Left / Right
    public void MoveLeftDown()  { horizontal = -1f; }
    public void MoveLeftUp()    { horizontal = 0f; }

    public void MoveRightDown() { horizontal = 1f; }
    public void MoveRightUp()   { horizontal = 0f; }

    // Accelerate / Brake
    public void AccelerateDown() { vertical = 1f; }
    public void AccelerateUp()   { vertical = 0f; }

    public void BrakeDown() { vertical = -1f; }
    public void BrakeUp()   { vertical = 0f; }

    // Optional: reset values when scene unloads
    void OnDisable()
    {
        horizontal = 0f;
        vertical = 0f;
    }
}
