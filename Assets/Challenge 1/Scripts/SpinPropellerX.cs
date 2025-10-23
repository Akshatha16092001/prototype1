using UnityEngine;

public class SpinPropellerX : MonoBehaviour
{
    public float spinSpeed = 1000f; // Adjust in Inspector for faster/slower spin

    void Update()
    {
        // Spin the propeller every frame around its Z axis
        transform.Rotate(Vector3.forward * spinSpeed * Time.deltaTime);
    }
}
