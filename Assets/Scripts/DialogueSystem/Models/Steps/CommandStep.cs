using Ontoverse.Console;

namespace Ontoverse.DialogueSystem
{
    public class CommandStep : BaseStep
    {
        public override EStepType StepType => EStepType.Command;
        public ECommandType CommandType = ECommandType.Unknown;

        public CommandStep()
        {
        }
    }
}
