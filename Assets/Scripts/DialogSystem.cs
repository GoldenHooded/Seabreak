using UnityEngine;
using UnityEngine.Events;

public partial class DialogSystem : MonoBehaviour
{
    public DialogString activeDialogString;
    private Dialog currentDialog;
    private int currentDialogIndex;
    private DialogString lastDialogString;
    private PlayerController playerController;
    public UnityEvent[] events;

    private void Awake()
    {
        playerController = FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
        if (activeDialogString != lastDialogString && activeDialogString != null)
        {
            StartDialogFromString(0);

            if (lastDialogString == null)
            {
                openDialogSound.Play();
            }
        }

        lastDialogString = activeDialogString;

        if (currentDialog != null)
        {
            HandleInput();
        }
    }

    public void StartDialogFromString(int index)
    {
        currentDialogIndex = index;

        if (playerController != null) playerController.canMove = false;
        currentDialog = activeDialogString.dialogs[currentDialogIndex];
        DisplayCurrentDialog();
    }

    private void HandleInput()
    {
        DialogInput input = GetPlayerInput();

        if (input != null)
        {
            if (input.inputType == DialogInput.InputType.Continue)
            {
                EndDialog();
            }
            else if (input.inputType == DialogInput.InputType.Answer)
            {

                var selectedAnswer = currentDialog.answers[input.answerIndex];
                if (selectedAnswer.unityEventIndex != -1)
                {
                    events[selectedAnswer.unityEventIndex].Invoke();
                }
                if (selectedAnswer.nextDialogString != null)
                {
                    activeDialogString = selectedAnswer.nextDialogString;
                }
                else
                {
                    Debug.Log("EndDialog");
                    EndDialog();
                }
            }
        }
    }

    private void DisplayCurrentDialog()
    {
        if (currentDialog != null)
        {
            SetText(currentDialog.text);
            SetAnswers(currentDialog.answers);
        }
    }
}
