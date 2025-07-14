using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DialogueFlowController
{
    public DialogueNode currentNode;
    private DialogueGraph graph;

    public event Action<DialogueStep> OnTextOutput;
    public event Action<List<DialogueButtonData>> OnButtonsPresented;
    public event Action OnCommandRequired;
    public event Action OnFlowEnded;

    private bool waitingForTyper = false;

    public DialogueFlowController(DialogueGraph loadedGraph)
    {
        graph = loadedGraph;
        currentNode = graph.GetNodeById(graph.EntryNodeId);
    }

    public void StartFlow()
    {
        if (currentNode != null)
            ExecuteCurrentStep();
        else
            Debug.LogError("No entry node found.");
    }

    public string SelectButton(int index)
    {
        if (!currentNode.Step.PresentButtons || index < 0 || index >= currentNode.Step.Buttons.Count)
        {
            Debug.LogWarning("Invalid button index.");
            return null;
        }

        var selectedButton = currentNode.Step.Buttons[index];
        var label = selectedButton.Label;

        var targetId = selectedButton.TargetNodeId;
        if (!string.IsNullOrEmpty(targetId))
        {
            currentNode = graph.GetNodeById(targetId);
            ExecuteCurrentStep();
        } else
        {
            Debug.LogWarning("Button has no linked target node.");
        }

        return label;
    }

    public void ExecuteCurrentStep()
    {
        var step = currentNode.Step;

        if (step.ClearBefore)
            OnTextOutput?.Invoke(null);

        waitingForTyper = true;
        OnTextOutput?.Invoke(step);
    }

    public void NotifyStepFinished()
    {
        if (!waitingForTyper)
            return;

        waitingForTyper = false;

        var step = currentNode.Step;

        if (step.PresentButtons)
        {
            OnButtonsPresented?.Invoke(step.Buttons);
        } else if (step.WaitForCommand)
        {
            OnCommandRequired?.Invoke();
        } else if (!step.RequiresUserInput)
        {
            Continue();
        }
    }

    public void Continue()
    {
        if (currentNode.Step.PresentButtons)
            return;

        if (currentNode.NextNodeIds.Count > 0)
        {
            currentNode = graph.GetNodeById(currentNode.NextNodeIds[0]);
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
