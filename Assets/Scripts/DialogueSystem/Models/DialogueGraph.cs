using System.Collections.Generic;
using System.Linq;

namespace Ontoverse.DialogueSystem
{
    public class DialogueGraph
    {
        public List<DialogueNode> Nodes = new();
        public string EntryNodeId;

        public DialogueNode GetNodeById(string id) => Nodes.Find(n => n.Id == id);

        public DialogueNode GetPreviousStep(DialogueNode node)
        {
            var prev = Nodes.Find(n => n.NextNodeId == node.Id);
            if (prev != null) return prev;

            return Nodes.Find(n =>
                n.Step is ButtonStep bs && bs.Buttons.Any(b => b.TargetNodeId == node.Id)
            );
        }
    }
}