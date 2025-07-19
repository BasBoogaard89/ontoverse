using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueFlowController
{
    public DialogueNode CurrentNode { get; private set; }
    private readonly DialogueGraph graph;
    private readonly MonoBehaviour coroutineRunner;
    private readonly bool enableDelays;

    private bool waitingForStepToComplete;
    private bool isRetrying;
    private CommandStep retryStep;

    public event Action<BaseStep> OnStepOutput;
    public event Action<List<ButtonData>> OnButtonsPresented;
    public event Action OnCommandRequired;
    public event Action<ActionStep> OnActionRequested;
    public event Action OnFlowEnded;

    public DialogueFlowController(DialogueGraph loadedGraph, MonoBehaviour coroutineHost, bool enableDelays)
    {
        graph = loadedGraph ?? throw new ArgumentNullException(nameof(loadedGraph));
        CurrentNode = graph.GetNodeById(graph.EntryNodeId);
        coroutineRunner = coroutineHost ?? throw new ArgumentNullException(nameof(coroutineHost));
        this.enableDelays = enableDelays;
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
            case WaitStep ws:
                coroutineRunner.StartCoroutine(WaitAndContinue(ws.DelayConfig.DelayBefore));
                break;

            case TypeStep ts:
                waitingForStepToComplete = true;
                coroutineRunner.StartCoroutine(HandleTypeStep(ts));
                break;

            case ButtonStep bs:
                OnStepOutput?.Invoke(bs);
                OnButtonsPresented?.Invoke(bs.Buttons);
                break;

            case CommandStep cs:
                waitingForStepToComplete = true;
                OnStepOutput?.Invoke(cs);
                OnCommandRequired?.Invoke();
                break;

            case ActionStep a:
                OnActionRequested?.Invoke(a);
                break;

            default:
                OnFlowEnded?.Invoke();
                break;
        }
    }

    private IEnumerator HandleTypeStep(TypeStep step)
    {
        if (enableDelays && step.DelayConfig.DelayBefore > 0f)
            yield return new WaitForSeconds(step.DelayConfig.DelayBefore);

        OnStepOutput?.Invoke(step);
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
                OnCommandRequired?.Invoke();
            }
            return;
        }

        if (CurrentNode.Step is TypeStep ts && enableDelays && ts.DelayConfig.DelayAfter > 0f)
        {
            coroutineRunner.StartCoroutine(WaitAndContinue(ts.DelayConfig.DelayAfter));
            return;
        }

        Continue();
    }

    public void SubmitCommand(string cmd)
    {
        if (CurrentNode.Step is CommandStep cs)
        {
            var validator = new CommandValidator();
            if (validator.Validate(cmd, cs))
            {
                waitingForStepToComplete = false;
                Continue();
            } else
            {
                var prevNode = graph.GetPreviousStep(CurrentNode);
                if (prevNode?.Step is TypeStep context)
                {
                    retryStep = cs;
                    isRetrying = true;
                    waitingForStepToComplete = true;
                    coroutineRunner.StartCoroutine(HandleTypeStep(context));
                } else
                {
                    waitingForStepToComplete = true;
                    OnStepOutput?.Invoke(cs);
                    OnCommandRequired?.Invoke();
                }
            }
        }
    }

    public void SelectButton(int index)
    {
        if (CurrentNode.Step is ButtonStep bs && index >= 0 && index < bs.Buttons.Count)
        {
            var target = bs.Buttons[index].TargetNodeId;
            if (!string.IsNullOrEmpty(target))
            {
                CurrentNode = graph.GetNodeById(target);
                ExecuteCurrentStep();
            }
        }
    }

    public void Continue()
    {
        if (!string.IsNullOrEmpty(CurrentNode.NextNodeId))
        {
            CurrentNode = graph.GetNodeById(CurrentNode.NextNodeId);
            ExecuteCurrentStep();
        } else
            OnFlowEnded?.Invoke();
    }

    private IEnumerator WaitAndContinue(float delay)
    {
        if (enableDelays && delay > 0f)
            yield return new WaitForSeconds(delay);

        Continue();
    }
}
