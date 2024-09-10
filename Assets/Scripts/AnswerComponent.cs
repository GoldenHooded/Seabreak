using TMPro;
using UnityEngine;

public class AnswerComponent : MonoBehaviour
{
    public bool selected;
    public string text;
    public TMP_Text tmp_text;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void LateUpdate()
    {
        anim.SetBool("Selected", selected);
        tmp_text.text = text;
    }
}