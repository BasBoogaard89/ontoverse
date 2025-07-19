using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ConsoleButtonManager : MonoBehaviour
{
    [SerializeField] VisualTreeAsset buttonTemplate;

    VisualElement buttonElement;
    VisualElement buttonContainer;
    VisualElement scrollContent;

    public event Action<int, string> OnButtonSelected;

    void Awake()
    {
        scrollContent = GetComponent<UIDocument>().rootVisualElement.Q("unity-content-container");

        buttonElement = buttonTemplate.Instantiate();
        buttonContainer = buttonElement.Q<VisualElement>("button-container");
    }

    public void ShowButtons(List<ButtonData> buttons)
    {
        if (buttonElement.parent != null)
            buttonElement.RemoveFromHierarchy();

        buttonContainer.Clear();
        scrollContent.Add(buttonElement);

        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i;
            var data = buttons[i];

            var btn = new Button(() => OnButtonClick(index, data.Label))
            {
                text = data.Label
            };

            buttonContainer.Add(btn);
        }
    }

    void Hide()
    {
        if (buttonElement.parent != null)
            buttonElement.RemoveFromHierarchy();
    }

    void OnButtonClick(int index, string label)
    {
        Hide();
        OnButtonSelected?.Invoke(index, label);
    }
}
