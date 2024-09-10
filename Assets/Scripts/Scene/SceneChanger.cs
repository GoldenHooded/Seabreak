using UnityEngine;

public class SceneChanger : MonoBehaviour
{
    public static void PlaySceneChangeAnim()
    {
        SceneChanger sc = FindFirstObjectByType<SceneChanger>();
        sc.GetComponent<Animator>().SetTrigger("ChangeScene");
        sc.GetComponent<AudioSource>().Play();
    }
}
