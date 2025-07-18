using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueNode
{
    public string Id;
    public DialogueStep Step;
    public List<string> NextNodeIds = new();
    public float PositionX;
    public float PositionY;

    [JsonIgnore]
    public Vector2 Position => new(PositionX, PositionY);

    public DialogueNode()
    {
        Id = Guid.NewGuid().ToString();
        Step = new DialogueStep(EDialogueStepType.None);
    }

    public DialogueNode(string id)
    {
        Id = id;
        Step = new DialogueStep(EDialogueStepType.None);
    }
}