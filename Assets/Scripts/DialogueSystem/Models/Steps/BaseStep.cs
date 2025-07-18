using Newtonsoft.Json;

[JsonConverter(typeof(DialogueStepConverter))]
public abstract class BaseStep
{
    public abstract EStepType StepType { get; }
}