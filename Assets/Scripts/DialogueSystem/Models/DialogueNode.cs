using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Ontoverse.DialogueSystem
{
    public class DialogueNode
    {
        public string Id;
        public BaseStep Step;
        public string NextNodeId;
        public float PositionX;
        public float PositionY;
        public float Width;
        public float Height;

        [JsonIgnore]
        public Vector2 Position => new(PositionX, PositionY);

        public DialogueNode()
        {
            Id = Guid.NewGuid().ToString();
        }

        public DialogueNode(string id)
        {
            Id = id;
        }
    }
}