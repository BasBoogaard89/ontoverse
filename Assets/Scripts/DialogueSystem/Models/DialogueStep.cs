using System;
using System.Collections.Generic;

[Serializable]
public class DialogueStep
{
    public EDialogueStepType StepType;
    public ELogType LogType;
    public string Text;
    public float Delay;
    public float CharacterDelay;
    public bool ClearBefore;

    public bool RequiresUserInput;
    public bool WaitForCommand;
    public ECommandType CommandType = ECommandType.Unknown;
    public bool PresentButtons;
    public List<DialogueButtonData> Buttons = new();

    public int SelectedButtonIndex = -1;

    public DialogueStep(EDialogueStepType type, string text = "", ELogType logType = ELogType.None)
    {
        StepType = type;
        LogType = logType;
        Text = text;
    }
}
