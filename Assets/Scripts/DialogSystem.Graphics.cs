using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public partial class DialogSystem : MonoBehaviour
{
    [Space]
    public Animator anim;
    public TMP_Text text;
    public GameObject answerPref;
    public Transform answerParent;
    public AudioSource openDialogSound;
    public AudioSource closeDialogSound;
    public AudioSource talkSound;
    public AudioSource selectSound;
    public AudioSource changeOptionSound;

    private string currentDialogText;
    private bool isTextwritten;
    private List<AnswerComponent> activeACs;
    private int selectedACIndex;

    private void EndDialog()
    {
        if (currentDialog.unityEventIndex != -1)
        {
            events[currentDialog.unityEventIndex].Invoke();
        }

        currentDialog = null;

        currentDialogIndex++;
        if (currentDialogIndex < activeDialogString.dialogs.Length && activeDialogString.dialogs[currentDialogIndex] != null)
        {
            StartDialogFromString(currentDialogIndex);
            return;
        }

        activeDialogString = null;
        closeDialogSound.Play();
        anim.SetBool("Active", currentDialog != null);
        playerController.canMove = true;
    }

    private void SetText(string text)
    {
        currentDialogText = text;
        anim.SetBool("Active", currentDialog != null);
        StopAllCoroutines();
        StartCoroutine(WriteText());
    }

    private void SetAnswers(Answer[] answers)
    {
        if (activeACs != null)
        {
            for (int i = 0; i < activeACs.Count; i++)
            {
                if (activeACs[i] != null)
                    Destroy(activeACs[i].gameObject);
            }
        }

        activeACs = new();

        foreach (Answer answer in answers) 
        {
            AnswerComponent component = Instantiate(answerPref, answerParent).GetComponent<AnswerComponent>();
            component.text = answer.text;

            activeACs.Add(component);
        }
    }

    private IEnumerator WriteText()
    {
        // Limpia el texto antes de comenzar
        text.text = "";
        isTextwritten = false;

        // Tiempo entre la aparición de cada carácter
        float typingSpeed = 0.05f; // Puedes ajustar este valor para cambiar la velocidad de escritura

        foreach (char letter in currentDialogText.ToCharArray())
        {
            if (text.text == currentDialogText) break;

            // Agrega el siguiente carácter al texto
            text.text += letter;

            talkSound.Play();

            // Espera el tiempo determinado antes de añadir el siguiente carácter
            yield return new WaitForSeconds(typingSpeed);
        }

        isTextwritten = true;
        // Aquí podrías agregar una pausa adicional o esperar un input antes de proceder
        // yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
    }

    private void LateUpdate()
    {
        bool aPBool = activeACs != null && activeACs.Count > 0 && isTextwritten;

        anim.SetBool("answerParent", aPBool);
    }

    ////

    private DialogInput GetPlayerInput()
    {
        // Aquí agregarías la lógica para determinar si el jugador quiere continuar o seleccionar una respuesta.
        // Para ejemplo, solo devuelve un "continue"
        // Esta lógica debe ser reemplazada con la detección real del input del jugador

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return)) // Por ejemplo, si presiona Espacio, continua el diálogo
        {
            anim.SetTrigger("Press");

            if (!isTextwritten)
            {
                StopCoroutine(WriteText());
                text.text = currentDialogText;
                isTextwritten = true;

                return null;
            }
            else if (activeACs.Count > 0)
            {
                selectSound.Play();
                return new DialogInput(DialogInput.InputType.Answer, selectedACIndex);
            }

            return new DialogInput(DialogInput.InputType.Continue);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            selectedACIndex++;

            if (activeACs.Count > 0)
            {
                changeOptionSound.Play();
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            selectedACIndex--;

            changeOptionSound.Play();
        }

        SelectedACIndexLogic();

        return null;
    }

    private void SelectedACIndexLogic()
    {
        if (selectedACIndex < 0)
        {
            selectedACIndex = activeACs.Count - 1;
        }

        if (selectedACIndex >= activeACs.Count)
        {
            selectedACIndex = 0;
        }

        foreach (var ac in activeACs)
        {
            ac.selected = false;
        }

        if (activeACs.Count > 0)
            activeACs[selectedACIndex].selected = true;
    }
}