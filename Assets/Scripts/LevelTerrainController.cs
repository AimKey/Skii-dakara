using System;
using UnityEngine;
using UnityEngine.Serialization;

public class LevelTerrainController : MonoBehaviour
{
    [SerializeField] private SurfaceEffector2D surfaceEffector;

    [FormerlySerializedAs("defaultSpeed")] public float baseSpeed = 25f;

    public float currentSpeed;

    // Singleton instance
    public static LevelTerrainController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        surfaceEffector = GetComponent<SurfaceEffector2D>();
        currentSpeed = baseSpeed;
    }

    public void SetSurfaceSpeed(float speed)
    {
        surfaceEffector.speed = speed;
    }

    public void ResetSurfaceSpeed()
    {
        surfaceEffector.speed = baseSpeed;
    }
}