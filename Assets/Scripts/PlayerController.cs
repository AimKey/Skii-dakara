using Assets.Scripts.Constants;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public bool isGrounded;
    [SerializeField] public Rigidbody2D rb;

    // Rotation related variables
    [SerializeField] private float rotationSpeed = 360; // Speed of rotation in degrees per second
    public InputAction rotateRightAction;
    public InputAction rotateLeftAction;

    // Jumping related variables
    public float jumpForce = 10f;
    private bool bufferJump = false;
    [SerializeField] private float bufferTime = 0.2f; // Time in seconds to buffer the jump
    private float bufferRemainingTime;


    // Particle prefabs
    [SerializeField] private ParticleSystem driftingParticlePrefab;
    [SerializeField] private GameObject deadParticlePrefab;
    [SerializeField] private ParticleSystem speedBoostParticlePrefab;

    // Trick related variables
    private float RotatedDegree = 0;

    // Boost related variables
    [SerializeField] private float boostDuration = 2f;
    [SerializeField] private float boostMultiplier = 2f;
    [SerializeField] private AnimationCurve boostCurve; // ease-in-out curve
    private bool didFlip = false;
    public bool isBoosting = false;
    private float boostTimer;
    private LevelTerrainController levelTerrainController;

    // Child collider references
    [SerializeField] private Collider2D GameOverCollider;

    public bool IsPlayerLost = false;

    // Instance var
    public static PlayerController Instance { get; private set; }

    // Sound clips
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip crashSound;
    [SerializeField] private AudioClip trickPerformedSound;

    // Surface normal tst
    private Vector2 surfaceNormal = Vector2.up;

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
    }

    void Start()
    {
        rb = rb.GetComponent<Rigidbody2D>();
        bufferRemainingTime = bufferTime;

        // Initialize input actions
        rotateRightAction.Enable();
        rotateLeftAction.Enable();
        levelTerrainController = LevelTerrainController.Instance;

        // Init sound
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        DecreaseJumpBufferTime();
    }

    private void FixedUpdate()
    {
        HandlePlayerRotation();
        HandleBoostPlayer();
        ApplyConstantForce();
    }

    private void ApplyConstantForce()
    {
        Debug.Log("Applying constant force to player");
        rb.AddForce(-surfaceNormal * 100f, ForceMode2D.Force);
    }

    private void HandlePlayerRotation()
    {
        // If on the ground don't allow rotation
        if (isGrounded)
        {
            return;
        }

        // Apply rotation based on input
        float rotationInput = rotateLeftAction.ReadValue<float>() - rotateRightAction.ReadValue<float>();
        float rotationAmount = 0f;
        if (rotationInput != 0f)
        {
            rotationAmount = rotationInput * rotationSpeed * Time.deltaTime;
            rb.MoveRotation(rb.rotation + rotationAmount);
        }

        // Handle player performing tricks related to rotations
        TrickHandler(rotationAmount);
    }

    private void TrickHandler(float rotationAmount)
    {
        RotatedDegree += rotationAmount;
        if (Math.Abs(RotatedDegree) >= 300 && !isGrounded)
        {
            if (RotatedDegree <= -300)
            {
                didFlip = true;
                GameManager.Instance.IncreaseScoreFromPlayerTrick(GameConstants.FrontFlip);
                audioSource.PlayOneShot(trickPerformedSound);
            }
            else if (RotatedDegree >= 300)
            {
                didFlip = true;
                GameManager.Instance.IncreaseScoreFromPlayerTrick(GameConstants.BackFlip);
                audioSource.PlayOneShot(trickPerformedSound);
            }

            // Reset
            RotatedDegree = 0f;
        }
    }

    // Used by rock controller
    public void RampingRockTrickHandler()
    {
        GameManager.Instance.IncreaseScoreFromPlayerTrick(GameConstants.RockSmash);
    }

    private void HandleBoostPlayer()
    {
        if (isBoosting)
        {
            boostTimer -= Time.fixedDeltaTime;
            if (boostTimer <= 0)
            {
                isBoosting = false;
                boostTimer = boostDuration;
                speedBoostParticlePrefab.Stop();
            }

            // Boosting stage
            float t = 1f - (boostTimer / boostDuration);

            // Explanation:
            // 1f is the base speed
            // Ex: base speed: 1f, boostMultiplier: 2f => The additional speed is 1f
            // So we want to manipulate the additional speed, not the boost multiplier itself
            float multiplier = 1f + boostCurve.Evaluate(t) * (boostMultiplier - 1f);

            levelTerrainController.SetSurfaceSpeed(levelTerrainController.baseSpeed * multiplier);
        }
        else
        {
            levelTerrainController.SetSurfaceSpeed(levelTerrainController.baseSpeed);
        }
    }

    private void DecreaseJumpBufferTime()
    {
        if (bufferJump)
        {
            bufferRemainingTime -= Time.deltaTime;
            if (bufferRemainingTime <= 0f)
            {
                bufferJump = false; // Reset the buffer if time runs out
                bufferRemainingTime = 0.2f; // Reset the buffer time for next use
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Ground"))
        {
            isGrounded = true;
            // Check if we have a buffered jump
            if (bufferJump)
            {
                Debug.Log("Buffered jump executed");
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                bufferJump = false; // Reset the buffer after executing the jump
            }

            // Play the drifting particle effect that follow this player
            driftingParticlePrefab.Play();

            // Reset the rotated degree
            RotatedDegree = 0f;

            // Trigger boost if it is available
            if (didFlip && !IsPlayerLost)
            {
                isBoosting = true;
                boostTimer = boostDuration;
                didFlip = false;
                speedBoostParticlePrefab.Play();
            }

            surfaceNormal = other.contacts[0].normal;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.collider.CompareTag("Ground"))
        {
            isGrounded = false;
            driftingParticlePrefab.Stop();
            surfaceNormal = Vector2.up; // Reset to default
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            Debug.Log("Ouch, my head");
            PlayerGameOver();
        }
    }

    public void PlayerGameOver()
    {
        IsPlayerLost = true;
        rb.simulated = false;
        speedBoostParticlePrefab.Stop();
        Instantiate(deadParticlePrefab, transform.position, Quaternion.identity);
        audioSource.PlayOneShot(crashSound);
        GameManager.Instance.PlayerLost();
    }

    public void Jump()
    {
        if ((isGrounded))
        {
            Vector2 jumpVector = Vector2.up + Vector2.right * 0.1f; // Small forward push
            rb.AddForce(jumpVector.normalized * jumpForce, ForceMode2D.Impulse);
        }
        else
        {
            bufferJump = true;
        }
    }

    public void ToggleMainMenu()
    {
        GameManager.Instance.PauseGame();
    }
}