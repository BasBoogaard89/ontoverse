using System.Collections.Generic;

public class DialogueGraph
{
    public List<DialogueNode> Nodes = new();
    public string EntryNodeId;

    public DialogueNode GetNodeById(string id) => Nodes.Find(n => n.Id == id);
}
