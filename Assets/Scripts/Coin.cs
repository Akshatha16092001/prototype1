using UnityEngine;

public class Coin : MonoBehaviour
{
    public int coinValue = 10;
    public GameObject pickupEffect; // Assign CoinPickupEffect prefab here
    public AudioClip pickupSound;   // Assign sound clip here

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Add to score
            GameManager.Instance.AddCoinScore(coinValue);

            // ✨ Spawn pickup particle effect
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            //  Play sound at coin position
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // Destroy coin object
            Destroy(gameObject);
        }
    }
}
