using System;
using UnityEditor;
using UnityEngine.UIElements;

public class ButtonStepForm
{
    private readonly VisualElement buttonsList;
    private readonly Button addButton;
    private readonly VisualTreeAsset rowTemplateAsset;
    private EventCallback<ClickEvent> addCallback;
    private int maxButtonsCount = 5;

    public ButtonStepForm(VisualElement root)
    {
        buttonsList = root.Q<VisualElement>("buttonsList");
        addButton = root.Q<Button>("addButton");

        rowTemplateAsset = AssetHelper.LoadAsset<VisualTreeAsset>("/Views/ButtonView.uxml");
    }

    public void Setup(ButtonStep step, Action<int> onLink, Action<int> onRemove)
    {
        buttonsList.Clear();

        for (int i = 0; i < step.Buttons.Count; i++)
        {
            int index = i;
            var data = step.Buttons[i];

            var row = rowTemplateAsset.CloneTree();

            var labelField = row.Q<TextField>("labelField");
            labelField.value = data.Label;
            labelField.RegisterValueChangedCallback(evt => data.Label = evt.newValue);

            var linkButton = row.Q<Button>("linkButton");
            linkButton.clicked += () => onLink(index);

            var removeButton = row.Q<Button>("removeButton");
            removeButton.clicked += () => onRemove(index);

            buttonsList.Add(row);
        }

        if (addCallback != null)
            addButton.UnregisterCallback(addCallback);

        addCallback = evt =>
        {
            if (step.Buttons.Count >= maxButtonsCount)
                return;

            step.Buttons.Add(new ButtonData($"Option {step.Buttons.Count}"));
            Setup(step, onLink, onRemove);
        };
        addButton.RegisterCallback(addCallback);

        addButton.SetEnabled(step.Buttons.Count < maxButtonsCount);
    }
}
