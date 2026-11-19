using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Button actions for the Start and Game Over screens. Hook these up to Button OnClick.</summary>
public class MenuButtons : MonoBehaviour
{
    [SerializeField] string firstLevel = "_Scene_Lvl_0";
    [SerializeField] string mainMenuScene = "_Start_Scene";

    public void PlayGame()
    {
        GameSession.Reset();
        SceneManager.LoadScene(firstLevel);
    }

    public void OpenMainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
