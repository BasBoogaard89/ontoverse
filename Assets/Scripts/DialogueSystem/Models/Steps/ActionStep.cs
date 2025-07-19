using System.Collections.Generic;

public class ActionStep : BaseStep
{
    public override EStepType StepType => EStepType.Action;
    public EActionType ActionType;
    public Dictionary<string, object> Parameters = new();
}
