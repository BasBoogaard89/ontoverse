using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueFlowController
{
    public DialogueNode CurrentNode { get; private set; }
    private DialogueGraph graph;

    public event Action<BaseStep> OnStepOutput;
    public event Action<List<ButtonData>> OnButtonsPresented;
    public event Action OnCommandRequired;
    public event Action OnFlowEnded;

    private bool waitingForStepToComplete = false;

    public DialogueFlowController(DialogueGraph loadedGraph)
    {
        graph = loadedGraph;
        CurrentNode = graph.GetNodeById(graph.EntryNodeId);
    }

    public void StartFlow()
    {
        if (CurrentNode != null)
            ExecuteCurrentStep();
        else
            Debug.LogError("No entry node found.");
    }

    private void ExecuteCurrentStep()
    {
        if (CurrentNode?.Step == null)
        {
            Debug.LogWarning("Current node has no step.");
            OnFlowEnded?.Invoke();
            return;
        }

        waitingForStepToComplete = true;
        OnStepOutput?.Invoke(CurrentNode.Step);
    }

    public void NotifyStepFinished()
    {
        if (!waitingForStepToComplete)
            return;

        waitingForStepToComplete = false;

        if (CurrentNode.Step is ButtonStep btnStep)
        {
            OnButtonsPresented?.Invoke(btnStep.Buttons);
            return;
        }

        if (CurrentNode.Step is CommandStep cmdStep)
        {
            OnCommandRequired?.Invoke();
            return;
        }

        Continue();
    }

    public void SelectButton(int index)
    {
        if (CurrentNode.Step is not ButtonStep btnStep)
        {
            Debug.LogWarning("Current step is not a ButtonStep.");
            return;
        }

        if (index < 0 || index >= btnStep.Buttons.Count)
        {
            Debug.LogWarning("Invalid button index.");
            return;
        }

        var selected = btnStep.Buttons[index];
        if (!string.IsNullOrEmpty(selected.TargetNodeId))
        {
            CurrentNode = graph.GetNodeById(selected.TargetNodeId);
            ExecuteCurrentStep();
        } else
        {
            Debug.LogWarning("Button has no target.");
        }
    }

    public void SubmitCommand(string cmd)
    {
        //if (CurrentNode.Step is CommandStep cs)
        //{
        //    bool valid = cs.Validate(input);
        //    ResumeFlow(valid);
        //}

        //if (ValidateCommand(cmd, cmdStep))
        //{
        //    Continue();
        //} else
        //{
        //    ExecuteCurrentStep();
        //}
    }

    //private bool ValidateCommand(string input, CommandStep step)
    //{
    //    return input.Trim().Equals(step.ExpectedCommand, StringComparison.OrdinalIgnoreCase);
    //}

    public void Continue()
    {
        if (!string.IsNullOrEmpty(CurrentNode.NextNodeId))
        {
            CurrentNode = graph.GetNodeById(CurrentNode.NextNodeId);
            ExecuteCurrentStep();
        } else
        {
            OnFlowEnded?.Invoke();
        }
    }

    public void ResumeFlow(bool isValid)
    {
        if (isValid)
            Continue();
        else
            ExecuteCurrentStep();
    }
}
