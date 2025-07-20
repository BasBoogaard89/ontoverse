using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DialogueFlowController
{
    public DialogueNode CurrentNode { get; private set; }
    public DialogueGraph Graph { get; private set; }

    private readonly MonoBehaviour coroutineRunner;
    private readonly bool enableDelays;

    private bool waitingForStepToComplete;
    private bool isRetrying;
    private CommandStep retryStep;

    public event Action<BaseStep> OnStepOutput;
    public event Action<ActionStep> OnActionRequested;
    public event Action OnFlowEnded;

    public DialogueFlowController(DialogueGraph loadedGraph, MonoBehaviour coroutineHost, bool enableDelays)
    {
        coroutineRunner = coroutineHost ?? throw new ArgumentNullException(nameof(coroutineHost));
        this.enableDelays = enableDelays;
        LoadGraph(loadedGraph);
    }

    public void LoadGraph(DialogueGraph newGraph)
    {
        Graph = newGraph;
        CurrentNode = Graph.GetNodeById(Graph.EntryNodeId);

        waitingForStepToComplete = false;
        isRetrying = false;
        retryStep = null;

        StartFlow();
    }

    public void StartFlow()
    {
        if (CurrentNode != null)
            ExecuteCurrentStep();
        else
            OnFlowEnded?.Invoke();
    }

    private void ExecuteCurrentStep()
    {
        var step = CurrentNode?.Step;
        if (step == null)
        {
            Debug.LogWarning("No step found, ending flow.");
            OnFlowEnded?.Invoke();
            return;
        }

        switch (step)
        {
            case WaitStep waitStep:
                coroutineRunner.StartCoroutine(WaitAndContinue(waitStep.DelayConfig.DelayBefore));
                break;

            case TypeStep:
            case ButtonStep:
            case CommandStep:
                waitingForStepToComplete = true;
                OnStepOutput?.Invoke(step);
                break;

            case ActionStep actionStep:
                waitingForStepToComplete = true;
                OnActionRequested?.Invoke(actionStep);
                break;

            default:
                OnFlowEnded?.Invoke();
                break;
        }
    }

    public void NotifyStepFinished()
    {
        if (!waitingForStepToComplete)
            return;

        waitingForStepToComplete = false;

        if (isRetrying)
        {
            isRetrying = false;
            if (retryStep != null)
            {
                waitingForStepToComplete = true;
                OnStepOutput?.Invoke(retryStep);
            }
            return;
        }

        if (CurrentNode.Step is CommandStep)
            return;

        Continue();
    }

    public bool SubmitCommand(string cmd)
    {
        if (CurrentNode.Step is not CommandStep cs)
            return false;

        var validator = new CommandValidator();
        if (validator.Validate(cmd, cs))
        {
            waitingForStepToComplete = false;
            Continue();
            return true;
        }

        retryStep = cs;
        isRetrying = true;
        waitingForStepToComplete = true;
        return false;
    }

    public void SelectButton(int index)
    {
        if (CurrentNode.Step is ButtonStep bs && index >= 0 && index < bs.Buttons.Count)
        {
            var target = bs.Buttons[index].TargetNodeId;
            if (!string.IsNullOrEmpty(target))
            {
                CurrentNode = Graph.GetNodeById(target);
                ExecuteCurrentStep();
            }
        }
    }

    public void Continue()
    {
        if (!string.IsNullOrEmpty(CurrentNode.NextNodeId))
        {
            CurrentNode = Graph.GetNodeById(CurrentNode.NextNodeId);
            ExecuteCurrentStep();
        } else
        {
            OnFlowEnded?.Invoke();
        }
    }

    public void HandleAction(ActionStep step, bool result)
    {
        waitingForStepToComplete = false;

        if (!string.IsNullOrEmpty(step.NextNodeIdTrue) && !string.IsNullOrEmpty(step.NextNodeIdFalse))
        {
            CurrentNode = Graph.GetNodeById(result ? step.NextNodeIdTrue : step.NextNodeIdFalse);
        } else if (!string.IsNullOrEmpty(CurrentNode.NextNodeId))
        {
            CurrentNode = Graph.GetNodeById(CurrentNode.NextNodeId);
        } else
        {
            OnFlowEnded?.Invoke();
            return;
        }

        ExecuteCurrentStep();
    }

    private IEnumerator WaitAndContinue(float delay)
    {
        if (enableDelays && delay > 0f)
            yield return new WaitForSeconds(delay);

        Continue();
    }

    public DialogueNode GetPreviousNode()
    {
        return Graph.GetPreviousStep(CurrentNode);
    }
}
