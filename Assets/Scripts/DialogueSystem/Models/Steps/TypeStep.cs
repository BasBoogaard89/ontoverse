public class TypeStep : BaseStep
{
    public override EStepType StepType => EStepType.Type;
    public EDisplayType DisplayType;
    public ELogType LogType;
    public string Text;
    public bool OverrideDefaultDelay;

    public StepDelayConfig DelayConfig = new();

    public TypeStep(EDisplayType displayType, string text = "", ELogType logType = ELogType.None)
    {
        DisplayType = displayType;
        LogType = logType;
        Text = text;
    }
}
