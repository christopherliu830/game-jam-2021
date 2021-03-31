using UnityEngine;
using UnityEngine.SceneManagement;

// Script for main menu functionality
public class MainMenu : MonoBehaviour
{
    
    [Tooltip("Build index for the first level")][SerializeField] private int targetBuildIndex = 1;
    
    public void Play()
    {
        SceneManager.LoadScene(targetBuildIndex);
    }
    
    public void Quit()
    {
        Application.Quit();
    }
}
