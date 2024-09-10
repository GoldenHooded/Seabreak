using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Dialog String", menuName = "DialogSystem/DialogString", order = 0)]
public class DialogString : ScriptableObject, IEnumerable<Dialog>
{
    public Dialog[] dialogs;

    public IEnumerator<Dialog> GetEnumerator()
    {
        // Aqu� devolvemos un enumerador que itera sobre la colecci�n dialogs.
        return ((IEnumerable<Dialog>)dialogs).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        // Esta implementaci�n expl�cita de IEnumerable utiliza la implementaci�n gen�rica anterior.
        return GetEnumerator();
    }
}

[Serializable]
public class Dialog
{
    [TextArea]
    public string text;
    public Answer[] answers;
    public int unityEventIndex = -1;

    public Dialog() { }
    public Dialog(string text, Answer[] answers, int nextEvent = -1)
    {
        this.text = text;
        this.answers = answers;
        this.unityEventIndex = nextEvent;  // Inicializamos nextEvent tambi�n, en caso de que se necesite.
    }
}

[Serializable]
public class Answer
{
    public string text;
    public int unityEventIndex = -1;
    public DialogString nextDialogString;

    public Answer(string text, DialogString nextDialogString, int nextEvent = -1)
    {
        this.text = text;
        this.unityEventIndex = nextEvent;
        this.nextDialogString = nextDialogString;
    }
}
