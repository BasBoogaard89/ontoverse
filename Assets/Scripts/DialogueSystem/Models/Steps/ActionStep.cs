using System;
using System.Collections.Generic;

namespace Ontoverse.DialogueSystem
{
    public class ActionStep : BaseStep
    {
        public override EStepType StepType => EStepType.Action;
        public EActionType ActionType;
        public Dictionary<string, object> Parameters = new();
        public string NextNodeIdTrue;
        public string NextNodeIdFalse;

        public int GetInt(string key) => int.Parse((string)Parameters[key]);
        public bool GetBool(string key) => bool.Parse((string)Parameters[key]);
        public string GetString(string key) => (string)Parameters[key];

        public T GetParam<T>(string key)
        {
            if (Parameters.TryGetValue(key, out var value))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            throw new KeyNotFoundException($"Parameter '{key}' not found");
        }
    }
}
