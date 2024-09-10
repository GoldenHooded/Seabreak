using UnityEngine;

public class ButtonFunctions : MonoBehaviour
{
    public void openUrl(string url)
    {
        Application.OpenURL(url);
    }
}
