using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ConsoleInputManager : MonoBehaviour
{
    [SerializeField] VisualTreeAsset inputTemplate;
    List<string> baseSuggestions = new() { "help", "clear", "run", "exit" };

    VisualElement inputElement;
    TextField inputField;
    VisualElement autocompleteList;
    Button runButton;
    VisualElement scrollContent;

    bool autocompleteEnabled = false;
    List<string> suggestions;
    List<Label> suggestionItems = new();
    int selectedIndex = -1;
    int maxVisibleSuggestions = 5;

    public event Action<string> OnCommandSubmitted;

    void Awake()
    {
        scrollContent = GetComponent<UIDocument>().rootVisualElement.Q("unity-content-container");
        suggestions = new List<string>(baseSuggestions);
        suggestions.Sort(StringComparer.OrdinalIgnoreCase);

        InitUI();
    }

    void InitUI()
    {
        inputElement = inputTemplate.Instantiate();
        inputField = inputElement.Q<TextField>("command-input");
        autocompleteList = inputElement.Q<VisualElement>("autocomplete-list");
        runButton = inputElement.Q<Button>("run-button");

        inputField.RegisterCallback<FocusInEvent>(OnFocus);
        inputField.RegisterValueChangedCallback(OnInputChanged);
        inputField.RegisterCallback<KeyDownEvent>(OnKeyDown);
        inputField.RegisterCallback<FocusOutEvent>(_ =>
        {
            inputField.schedule.Execute(HideSuggestions).ExecuteLater(100);
        });

        runButton.clicked += () => RunCommand(inputField.value);

        SetSuggestions(baseSuggestions);
        HideSuggestions();
    }

    public void ShowInput()
    {
        if (inputElement.parent != null)
            inputElement.RemoveFromHierarchy();

        scrollContent.Add(inputElement);
    }

    void SetSuggestions(List<string> newSuggestions)
    {
        suggestions = new List<string>(newSuggestions);
        suggestions.Sort(StringComparer.OrdinalIgnoreCase);
    }

    void OnFocus(FocusInEvent evt)
    {
        inputField.schedule.Execute(() =>
        {
            inputField.SelectRange(inputField.value.Length, inputField.value.Length);
        }).ExecuteLater(100);
        evt.StopPropagation();
    }

    void OnInputChanged(ChangeEvent<string> evt)
    {
        string input = evt.newValue;

        if (!autocompleteEnabled || string.IsNullOrWhiteSpace(input))
        {
            HideSuggestions();
            return;
        }

        var matches = suggestions.FindAll(s => s.StartsWith(input, StringComparison.OrdinalIgnoreCase));
        if (matches.Count == 0)
        {
            HideSuggestions();
            return;
        }

        ShowSuggestions(matches);
    }

    void OnKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Backspace || (evt.character != '\0' && !char.IsControl(evt.character)))
            autocompleteEnabled = true;

        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
        {
            if (autocompleteList.style.display != DisplayStyle.None &&
                selectedIndex >= 0 &&
                selectedIndex < suggestionItems.Count)
            {
                evt.StopPropagation();
                SetInput(suggestionItems[selectedIndex].text);
            } else
            {
                evt.StopPropagation();
                RunCommand(inputField.value);
            }
            return;
        }

        if (autocompleteList.style.display == DisplayStyle.None)
            return;

        if (evt.keyCode == KeyCode.DownArrow)
        {
            evt.StopPropagation();
            MoveSelection(1);
        } else if (evt.keyCode == KeyCode.UpArrow)
        {
            evt.StopPropagation();
            MoveSelection(-1);
        }
    }

    void SetInput(string text)
    {
        inputField.value = text;
        inputField.Focus();
        HideSuggestions();
        autocompleteEnabled = false;
    }

    void ShowSuggestions(List<string> matches)
    {
        autocompleteList.Clear();
        suggestionItems.Clear();
        selectedIndex = -1;

        var limitedMatches = matches.Count > maxVisibleSuggestions
            ? matches.GetRange(0, maxVisibleSuggestions)
            : matches;

        foreach (var match in limitedMatches)
        {
            var item = new Label(match);
            item.AddToClassList("autocomplete-item");

            item.RegisterCallback<ClickEvent>(_ =>
            {
                SetInput(match);
                HideSuggestions();
            });

            item.RegisterCallback<MouseOverEvent>(_ =>
            {
                foreach (var l in suggestionItems)
                    l.RemoveFromClassList("highlight");

                item.AddToClassList("highlight");
                selectedIndex = suggestionItems.IndexOf(item);
            });

            autocompleteList.Add(item);
            suggestionItems.Add(item);
        }

        autocompleteList.style.display = DisplayStyle.Flex;
    }

    void HideSuggestions()
    {
        autocompleteList.style.display = DisplayStyle.None;
        selectedIndex = -1;
    }

    void MoveSelection(int direction)
    {
        if (suggestionItems.Count == 0)
            return;

        if (selectedIndex >= 0 && selectedIndex < suggestionItems.Count)
            suggestionItems[selectedIndex].RemoveFromClassList("highlight");

        selectedIndex += direction;

        if (selectedIndex < 0)
            selectedIndex = suggestionItems.Count - 1;
        else if (selectedIndex >= suggestionItems.Count)
            selectedIndex = 0;

        suggestionItems[selectedIndex].AddToClassList("highlight");
    }

    void Hide()
    {
        if (inputElement?.parent != null)
            inputElement.RemoveFromHierarchy();
    }

    void RunCommand(string cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd) || cmd.Equals("Enter a command..."))
            return;

        Hide();
        OnCommandSubmitted?.Invoke(cmd);
        inputField.value = "Enter a command...";

        HideSuggestions();
    }
}
