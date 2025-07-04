using Assets.Scripts.Constants;
using Assets.Scripts.Models;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public int PlayerScore = 0;

    [SerializeField] private int DefaultTrickScore = 1000;
    [SerializeField] private int RockTrickScore = 500;
    public int TrickScoreMultiplier = 1;
    public bool IsGamePaused = false;
    [SerializeField] private GameObject PauseMenu;
    [SerializeField] private GameObject GameOverMenu;
    
    [SerializeField] private GameObject WinMenu;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    public static GameManager Instance { get; set; }

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
    
    private void Start()
    {
        // Initialize the player score
        PlayerScore = 0;
        Time.timeScale = 1f;
    }
    
    // Method to increase the player's score then call UI to update it
    public void IncreaseScoreFromPlayerTrick(string trickType)
    {
        Trick trick = new Trick
        {
            TrickName = trickType,
            TrickScore = DefaultTrickScore * TrickScoreMultiplier,
        };

        switch (trickType)
        {
            case GameConstants.FrontFlip:
                UIController.Instance.DisplayPlayerTrick(trick);
                IncreaseScore(trick.TrickScore);
                break;
            case GameConstants.BackFlip:
                UIController.Instance.DisplayPlayerTrick(trick);
                IncreaseScore(trick.TrickScore);
                break;
            case GameConstants.RockSmash:
                trick.TrickName = "Rock Smash";
                trick.TrickScore = RockTrickScore * TrickScoreMultiplier;
                UIController.Instance.DisplayPlayerTrick(trick);
                IncreaseScore(trick.TrickScore);
                break;
            default:
                Debug.Log("Unknown trick type: " + trickType);
                break;
        }
    }

    private void IncreaseScore(int amount)
    {
        PlayerScore += amount;
        UIController.Instance.UpdatePlayerScore(PlayerScore);
    }

    // Method when player lost
    public void PlayerLost()
    {
        Debug.Log("Player lost the game.");
        GameOverMenu.SetActive(true);
    }

    // Method when player pause the game
    public void PauseGame()
    {
        if (IsGamePaused)
        {
            Time.timeScale = 1f;
            IsGamePaused = false;
            PauseMenu.SetActive(false);
        }
        else
        {
            Time.timeScale = 0f;
            IsGamePaused = true;
            PauseMenu.SetActive(true);
        }
    }
    
    public void BackToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    public void RetryGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level1");
    }
    
    public void PlayerWon()
    {
        // Stop the player script
        PlayerController.Instance.enabled = false;
        // Stop the surface and player movement
        LevelTerrainController.Instance.SetSurfaceSpeed(0f);
        // Show the win menu
        WinMenu.SetActive(true);
        // Update the final score text
        finalScoreText.text = $"Final Score: {PlayerScore:00000000}";
    }
}