using System;

[Serializable]
public class DialogueButtonData
{
    public string Id;
    public string Label;
    public string TargetNodeId;

    public DialogueButtonData(string label)
    {
        Id = Guid.NewGuid().ToString();
        Label = label;
        TargetNodeId = null;
    }
}
