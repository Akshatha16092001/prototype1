using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.AddCoinScore(value);
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Optional spin effect
        transform.Rotate(0, 90 * Time.deltaTime, 0);
    }
}
