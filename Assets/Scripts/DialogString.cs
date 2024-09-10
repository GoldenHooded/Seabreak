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
        // Aquí devolvemos un enumerador que itera sobre la colección dialogs.
        return ((IEnumerable<Dialog>)dialogs).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        // Esta implementación explícita de IEnumerable utiliza la implementación genérica anterior.
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
        this.unityEventIndex = nextEvent;  // Inicializamos nextEvent también, en caso de que se necesite.
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
