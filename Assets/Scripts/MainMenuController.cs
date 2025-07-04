using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void PlayGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level1");
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }

}
