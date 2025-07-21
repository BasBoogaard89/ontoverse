using Ontoverse.DialogueSystem;
using Ontoverse.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ontoverse.Console
{
    public class ConsoleTyper : MonoBehaviour
    {
        [SerializeField] private VisualTreeAsset lineTemplate;
        [HideInInspector] public bool enableDelays;

        private ScrollView scrollView;
        private VisualElement scrollContent;

        private readonly Queue<TypeLine> lineQueue = new();
        private bool isTyping = false;

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

        public void PrintLine(TypeStep step)
        {
            foreach (var line in step.Lines)
                lineQueue.Enqueue(line);

            if (!isTyping)
                StartCoroutine(ProcessLineQueue());
        }

        public void PrintUserLine(string text)
        {
            var userLine = new TypeLine(text, EDisplayType.UserInput, ELogType.User);
            PrintLine(new TypeStep { Lines = new List<TypeLine> { userLine } });
        }

        private IEnumerator ProcessLineQueue()
        {
            isTyping = true;

            while (lineQueue.Count > 0)
            {
                var line = lineQueue.Dequeue();
                yield return StartCoroutine(TypeLine(line));
            }

            isTyping = false;
            OnTyperComplete?.Invoke();
            OnStepComplete?.Invoke();
        }

        private IEnumerator TypeLine(TypeLine line)
        {
            if (enableDelays && line.DelayConfig.DelayBefore > 0f)
                yield return new WaitForSeconds(line.DelayConfig.DelayBefore);

            if (line.DisplayType == EDisplayType.Prompt)
            {
                yield return StartCoroutine(TypePrompt(line));
                yield break;
            }

            var element = CreateLineElement(line);
            var label = element.Q<Label>(LogTextClass);

            if (string.IsNullOrEmpty(line.Text))
                yield break;

            if (!enableDelays || line.DelayConfig.CharacterDelay <= 0f)
            {
                label.text += line.Text;
            } else
            {
                for (int i = 0; i < line.Text.Length; i++)
                {
                    label.text += line.Text[i];
                    yield return new WaitForSeconds(line.DelayConfig.CharacterDelay);
                }
            }

            ScrollToBottom();
        }

        private IEnumerator TypePrompt(TypeLine line, string prompt = "C:\\>", int blinkCount = 3, float blinkSpeed = 0.3f)
        {
            var element = CreateLineElement(line);
            var textLabel = element.Q<Label>(LogTextClass);

            string baseText = UIExtensions.GetDialogueColorTag(line.LogType) + prompt;

            for (int i = 0; i < blinkCount * 2; i++)
            {
                textLabel.text = baseText + (i % 2 == 0 ? "█" : " ");
                yield return new WaitForSeconds(blinkSpeed);
            }

            textLabel.text = baseText;

            if (!string.IsNullOrEmpty(line.Text))
            {
                for (int i = 0; i < line.Text.Length; i++)
                {
                    textLabel.text += line.Text[i];
                    if (enableDelays && line.DelayConfig.CharacterDelay > 0f)
                        yield return new WaitForSeconds(line.DelayConfig.CharacterDelay);
                }
            }

            ScrollToBottom();
        }

        private VisualElement CreateLineElement(TypeLine line)
        {
            var element = lineTemplate.Instantiate();
            var typeLabel = element.Q<Label>(LogTypeClass);
            var textLabel = element.Q<Label>(LogTextClass);

            bool isUserInput = line.DisplayType == EDisplayType.UserInput || line.LogType == ELogType.User;

            if (line.LogType == ELogType.None || isUserInput)
            {
                typeLabel.style.display = DisplayStyle.None;
            } else
            {
                typeLabel.text = UIExtensions.GetDialogueColorTag(line.LogType) +
                                 UIExtensions.GetDialogueStepTerminalPrefix(line.LogType);
                typeLabel.style.display = DisplayStyle.Flex;
            }

            textLabel.text = UIExtensions.GetDialogueColorTag(line.LogType) + (isUserInput ? "> " : "");

            if (isUserInput)
            {
                ResetActiveInputLine();
                activeInputLine = element;
            }

            scrollContent.Add(element);
            return element;
        }

        public void ScrollToBottom()
        {
            scrollView.schedule.Execute(() =>
            {
                scrollView.scrollOffset = new Vector2(0, float.MaxValue);
            }).ExecuteLater(1);
        }

        private void ResetActiveInputLine()
        {
            //if (activeInputLine == null) return;

            //var typeLabel = activeInputLine.Q<Label>(LogTypeClass);
            //var textLabel = activeInputLine.Q<Label>(LogTextClass);

            //if (typeLabel != null) typeLabel.text = UIExtensions.StripRichText(typeLabel.text);
            //if (textLabel != null) textLabel.text = UIExtensions.StripRichText(textLabel.text);

            //activeInputLine = null;

            Debug.Log("TODO: Reset active input line");
        }
    }
}