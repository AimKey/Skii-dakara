using System;
using UnityEngine;

public class GameOverDetector : MonoBehaviour
{
    // Reference to the parent GameObject
    [SerializeField] private PlayerController parentGameObject;
    
    // Reference to the dead particle prefab
    [SerializeField] private GameObject deadParticlePrefab;

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Ground"))
        {
            // Disable the parent GameObject to simulate game over
            parentGameObject.rb.simulated = false;
            Instantiate(deadParticlePrefab, transform.position, Quaternion.identity);
        }
    }
}
