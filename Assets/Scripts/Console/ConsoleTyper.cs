using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ConsoleTyper : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset lineTemplate;
    [SerializeField] private bool enableDelays = true;

    private ScrollView scrollView;
    private VisualElement scrollContent;

    private readonly Queue<TypeStep> stepQueue = new();
    private bool isRunningSequence = false;

    private VisualElement activeInputLine;

    private const string LogTypeClass = "log-type";
    private const string LogTextClass = "log-text";

    public event Action OnStepComplete;
    public event Action OnTyperComplete;

    void Awake()
    {
        scrollView = GetComponent<UIDocument>().rootVisualElement.Q<ScrollView>("ScrollView");
        scrollContent = scrollView.Q("unity-content-container");
    }

    //public void PrintLine(TypeStep step)
    //{
    //    stepQueue.Enqueue(step);
    //    if (!isRunningSequence)
    //        StartCoroutine(ProcessSteps());
    //}

    //public void PrintUserLine(string text)
    //{
    //    PrintLine(new DialogueStep(EStepType.FakeUserInput, text, ELogType.User));
    //}

    //public void ScrollToBottom()
    //{
    //    scrollView.schedule.Execute(() =>
    //    {
    //        scrollView.scrollOffset = new Vector2(0, float.MaxValue);
    //    }).ExecuteLater(1);
    //}

    //IEnumerator ProcessSteps()
    //{
    //    isRunningSequence = true;

    //    while (stepQueue.Count > 0)
    //    {
    //        var step = stepQueue.Dequeue();

    //        if (step.ClearBefore)
    //            scrollContent.Clear();

    //        if (enableDelays && step.Delay > 0)
    //            yield return new WaitForSeconds(step.Delay);

    //        switch (step.StepType)
    //        {
    //            case EStepType.Type:
    //                yield return StartCoroutine(TypeLine(step));
    //                break;
    //            case EStepType.FakeUserInput:
    //                yield return StartCoroutine(TypeLine(step, ">"));
    //                break;
    //            case EStepType.Prompt:
    //                yield return StartCoroutine(StartWithPrompt(step));
    //                break;
    //            case EStepType.Wait:
    //                break;
    //        }
    //    }

    //    isRunningSequence = false;
    //    OnTyperComplete?.Invoke();
    //    OnStepComplete?.Invoke();
    //}

    //IEnumerator TypeLine(TypeStep step, string prefix = null)
    //{
    //    var line = AddLineFromStep(step, prefix);
    //    var textLabel = line.Q<Label>(LogTextClass);

    //    if (step.CharacterDelay <= 0f || !enableDelays)
    //    {
    //        textLabel.text += step.Text;
    //    } else
    //    {
    //        for (int i = 0; i < step.Text.Length; i++)
    //        {
    //            textLabel.text += step.Text[i];
    //            yield return new WaitForSeconds(step.CharacterDelay);
    //        }
    //    }

    //    ScrollToBottom();
    //}

    //IEnumerator StartWithPrompt(TypeStep step, string prompt = "C:\\>", int blinkCount = 3, float blinkSpeed = 0.3f)
    //{
    //    var line = AddLineFromStep(step);
    //    var textLabel = line.Q<Label>(LogTextClass);

    //    for (int i = 0; i < blinkCount * 2; i++)
    //    {
    //        textLabel.text = UIExtensions.GetDialogueColorTag(step.LogType) + prompt + (i % 2 == 0 ? "█" : " ");
    //        yield return new WaitForSeconds(blinkSpeed);
    //    }

    //    textLabel.text = UIExtensions.GetDialogueColorTag(step.LogType) + prompt;
    //    yield return StartCoroutine(TypeOnExistingLabel(textLabel, step.Text, step.CharacterDelay));
    //}

    //IEnumerator TypeOnExistingLabel(Label label, string content, float characterDelay)
    //{
    //    for (int i = 0; i < content.Length; i++)
    //    {
    //        label.text += content[i];
    //        yield return new WaitForSeconds(characterDelay);
    //    }

    //    ScrollToBottom();
    //}

    //VisualElement AddLineFromStep(TypeStep step, string prefix = null)
    //{
    //    var line = lineTemplate.Instantiate();
    //    var typeLabel = line.Q<Label>(LogTypeClass);
    //    var textLabel = line.Q<Label>(LogTextClass);

    //    if (step.LogType == ELogType.None || step.LogType == ELogType.User)
    //        typeLabel.style.display = DisplayStyle.None;
    //    else
    //    {
    //        typeLabel.text = UIExtensions.GetDialogueColorTag(step.LogType) +
    //                         UIExtensions.GetDialogueStepTerminalPrefix(step);
    //        typeLabel.style.display = DisplayStyle.Flex;
    //    }

    //    textLabel.text = UIExtensions.GetDialogueColorTag(step.LogType) + (prefix ?? "");

    //    if (step.LogType == ELogType.Input || step.StepType == EStepType.FakeUserInput)
    //    {
    //        ResetActiveInputLine();
    //        activeInputLine = line;
    //    }

    //    scrollContent.Add(line);
    //    return line;
    //}

    //void ResetActiveInputLine()
    //{
    //    if (activeInputLine == null) return;

    //    var typeLabel = activeInputLine.Q<Label>(LogTypeClass);
    //    var textLabel = activeInputLine.Q<Label>(LogTextClass);

    //    if (typeLabel != null) typeLabel.text = UIExtensions.StripRichText(typeLabel.text);
    //    if (textLabel != null) textLabel.text = UIExtensions.StripRichText(textLabel.text);

    //    activeInputLine = null;
    //}
}
