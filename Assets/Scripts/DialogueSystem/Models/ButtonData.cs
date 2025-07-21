using System;

namespace Ontoverse.DialogueSystem
{
    public class ButtonData
    {
        public string Id;
        public string Label;
        public string TargetNodeId;

        public ButtonData(string label)
        {
            Id = Guid.NewGuid().ToString();
            Label = label;
            TargetNodeId = null;
        }
    }
}
