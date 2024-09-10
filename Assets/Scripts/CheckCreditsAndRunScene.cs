using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckCreditsAndRunScene : MonoBehaviour
{
    public int credits;
    public int scene;

    public void RunScene()
    {
        if (PlayerPrefs.GetInt("CreditsShown", 0) == 1)
        {
            SceneManager.LoadScene(scene);
        }
        else
        {
            SceneManager.LoadScene(credits);
        }
    }
}
