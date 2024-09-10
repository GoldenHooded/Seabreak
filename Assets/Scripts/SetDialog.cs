using UnityEngine;

public class SetDialog : MonoBehaviour
{
    public DialogString dialog;

    public DialogSystem dialogSystem;

    public void SetDialog_()
    {
        dialogSystem.activeDialogString = dialog;
    }
}
