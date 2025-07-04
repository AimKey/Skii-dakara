using System;
using Unity.Cinemachine;
using UnityEngine;

public class FinishLineController : MonoBehaviour
{
    [SerializeField] private GameObject finishVFX;
    [SerializeField] private AudioClip finishSound;
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player won");
            Instantiate(finishVFX, transform.position, Quaternion.identity);
            audioSource.PlayOneShot(finishSound);
            GameManager.Instance.PlayerWon();
        }
    }


}