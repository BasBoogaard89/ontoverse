using System;
using System.Collections.Generic;

public static class StepFactory
{
    public static readonly Dictionary<EStepType, Func<BaseStep>> Step = new()
    {
        { EStepType.Type, () => new TypeStep() },
        { EStepType.Wait, () => new WaitStep() },
        { EStepType.Button, () => new ButtonStep() },
        { EStepType.Command, () => new CommandStep() },
        { EStepType.Action, () => new ActionStep() },
    };
}
