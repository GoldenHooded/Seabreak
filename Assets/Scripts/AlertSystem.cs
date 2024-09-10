using TMPro;
using UnityEngine;

public class AlertSystem : MonoBehaviour
{
    public AudioSource alertSound;
    public TMP_Text alertText;
    public TMP_Text alertText2;
    public Animator anim;

    public static void SetAlert(string alert)
    {
        FindFirstObjectByType<AlertSystem>().SetAlert_(alert);
    }

    public void SetAlert_(string alert)
    {
        alertText.text = alert;
        alertText2.text = alert;
        alertSound.Play();
        anim.SetTrigger("Alert");
    }
}
