using System.Collections.Generic;

public class TypeStep : BaseStep
{
    public override EStepType StepType => EStepType.Type;
    public List<TypeLine> Lines = new();
}

public class TypeLine
{
    public EDisplayType DisplayType;
    public ELogType LogType;
    public string Text;
    public StepDelayConfig DelayConfig = new();

    public TypeLine(string text = "", EDisplayType displayType = EDisplayType.Type, ELogType logType = ELogType.None)
    {
        DisplayType = displayType;
        LogType = logType;
        Text = text;
    }
}