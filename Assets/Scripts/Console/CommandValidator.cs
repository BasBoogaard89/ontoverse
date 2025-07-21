using Ontoverse.DialogueSystem;
using System;
using System.Collections.Generic;

namespace Ontoverse.Console
{
    public class CommandValidator
    {
        private readonly Dictionary<ECommandType, Func<string, bool>> validators = new();

        public CommandValidator()
        {
            validators[ECommandType.Help] = cmd => cmd.Equals("help", StringComparison.OrdinalIgnoreCase);
            validators[ECommandType.Clear] = cmd => cmd.Equals("clear", StringComparison.OrdinalIgnoreCase);
            validators[ECommandType.List] = cmd => cmd.StartsWith("list", StringComparison.OrdinalIgnoreCase);
            //_validators[ECommandType.Exit] = cmd => cmd.Equals("exit", StringComparison.OrdinalIgnoreCase);
        }

        public bool Validate(string cmd, CommandStep step)
        {
            if (!validators.TryGetValue(step.CommandType, out var rule))
                return false;

            return rule(cmd);
        }

        public void AddCustomRule(ECommandType type, Func<string, bool> rule)
        {
            validators[type] = rule;
        }
    }
}