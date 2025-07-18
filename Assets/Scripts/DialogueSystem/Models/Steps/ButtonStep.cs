using System;
using System.Collections.Generic;

public class ButtonStep : BaseStep
{
    public override EStepType StepType => EStepType.Button;
    public List<ButtonData> Buttons = new();

    [NonSerialized]
    public int SelectedButtonIndex = -1;

    public ButtonStep()
    {
    }
}
