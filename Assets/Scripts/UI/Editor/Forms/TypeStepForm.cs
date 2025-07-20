using UnityEngine.UIElements;

public class TypeStepForm
{
    private readonly VisualElement linesList;
    private readonly Button addButton;
    private readonly VisualTreeAsset rowTemplate;

    private TypeStep currentStep;

    public TypeStepForm(VisualElement root)
    {
        linesList = root.Q<VisualElement>("linesList");
        addButton = root.Q<Button>("addButton");
        rowTemplate = AssetHelper.LoadEditorAsset<VisualTreeAsset>("/Views/TypeLineRowView.uxml");

        addButton.clicked += OnAddLineClicked;
    }

    public void Setup(TypeStep step)
    {
        currentStep = step;
        linesList.Clear();

        for (int i = 0; i < step.Lines.Count; i++)
        {
            int idx = i;
            var line = step.Lines[i];
            var row = rowTemplate.CloneTree();

            var binder = new FormBinder();
            binder.AutoBindFields(row, line);

            var removeButton = row.Q<Button>("removeButton");
            if (removeButton != null)
            {
                removeButton.clicked += () =>
                {
                    step.Lines.RemoveAt(idx);
                    Setup(step);
                };
            }

            linesList.Add(row);
        }
    }

    void OnAddLineClicked()
    {
        if (currentStep != null)
        {
            currentStep.Lines.Add(new TypeLine { Text = "", DisplayType = EDisplayType.Type, LogType = ELogType.System });
            Setup(currentStep);
        }
    }
}
