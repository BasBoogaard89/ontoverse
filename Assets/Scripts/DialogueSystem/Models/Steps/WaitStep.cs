namespace Ontoverse.DialogueSystem
{
    public class WaitStep : BaseStep
    {
        public override EStepType StepType => EStepType.Wait;
        public StepDelayConfig DelayConfig = new();
    }
}
