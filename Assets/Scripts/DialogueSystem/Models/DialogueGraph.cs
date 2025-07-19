using System.Collections.Generic;

public class DialogueGraph
{
    public List<DialogueNode> Nodes = new();
    public string EntryNodeId;

    public DialogueNode GetNodeById(string id) => Nodes.Find(n => n.Id == id);

    public DialogueNode GetPreviousStep(DialogueNode node)
    {
        foreach (var n in Nodes)
        {
            if (n.NextNodeId == node.Id)
                return n;
            if (n.Step is ButtonStep bs)
            {
                foreach (var btn in bs.Buttons)
                {
                    if (btn.TargetNodeId == node.Id)
                        return n;
                }
            }
        }

        return null;
    }
}
