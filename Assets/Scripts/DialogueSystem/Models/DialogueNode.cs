using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class DialogueNode
{
    public string Id;
    public DialogueStep Step;
    public List<string> NextNodeIds = new();
    public bool IsCollapsed = false;

    [NonSerialized] public Rect Rect;
    public Vector2 Position => Rect.position;

    public DialogueNode(string id, Rect rect)
    {
        Id = id;
        Rect = rect;
        Step = new DialogueStep(EDialogueStepType.None);
    }

    public void Drag(Vector2 delta)
    {
        Rect.position += delta;
    }
}