using System;
using System.Collections.Generic;

public class CommandValidator
{
    private Dictionary<ECommandType, Func<string, bool>> validators = new();

    public CommandValidator()
    {
        validators[ECommandType.Help] = cmd => cmd.Equals("help", StringComparison.OrdinalIgnoreCase);
        //_validators[ECommandType.Clear] = cmd => cmd.Equals("clear", StringComparison.OrdinalIgnoreCase);
        //_validators[ECommandType.Run] = cmd => cmd.StartsWith("run", StringComparison.OrdinalIgnoreCase);
        //_validators[ECommandType.Exit] = cmd => cmd.Equals("exit", StringComparison.OrdinalIgnoreCase);
    }

    public bool Validate(string cmd, CommandStep step)
    {
        //if (!validators.TryGetValue(step.CommandType, out var rule))
        //    return false;

        //return rule(cmd);
        return true;
    }

    public void AddCustomRule(ECommandType type, Func<string, bool> rule)
    {
        validators[type] = rule;
    }
}