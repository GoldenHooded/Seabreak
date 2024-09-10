public class DialogInput
{
    public enum InputType { Continue, Answer }
    
    public InputType inputType;
    public int answerIndex;

    public DialogInput(InputType inputType, int answerIndex = -1)
    {
        this.inputType = inputType;
        this.answerIndex = answerIndex;
    }
}
