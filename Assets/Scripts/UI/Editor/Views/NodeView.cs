using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeView : VisualElement
{
    public Action<NodeView> OnStartLink;
    public Action<NodeView> OnCompleteLink;
    public Action<NodeView> OnSelected;
    public Action<NodeView> OnRemovedNode;
    public Action<NodeView> OnMarkEntryNode;
    public Action<NodeView, int> OnRemovedButton;

    public DialogueNode Node;

    private VisualElement input, output, delete, formContainer, entryToggle;
    private Label title;
    private readonly string viewsPath = $"{AssetHelper.EditorUIPath}/Views";
    private string hoverClass = "hover";

    public NodeView(DialogueNode node, Action onPositionChanged)
    {
        Node = node;
        Node.Step ??= new TypeStep(EDisplayType.Type);
        Node.NextNodeId ??= null;

        var visualTree = AssetHelper.LoadAsset<VisualTreeAsset>("/Views/NodeView.uxml");
        visualTree.CloneTree(this);

        styleSheets.Add(AssetHelper.LoadAsset<StyleSheet>("Views/NodeView.uss"));

        SetPosition(Node.Position);

        input = this.Q("input");

        output = this.Q("output");
        title = this.Q<Label>("title");
        formContainer = this.Q("form-container");
        entryToggle = this.Q("entry-toggle");
        delete = this.Q("delete");

        input.RegisterCallback<ClickEvent>(_ => OnCompleteLink?.Invoke(this));
        output.RegisterCallback<ClickEvent>(_ => OnStartLink?.Invoke(this));
        delete.RegisterCallback<ClickEvent>(_ => OnRemovedNode?.Invoke(this));
        entryToggle.RegisterCallback<ClickEvent>(_ => OnMarkEntryNode?.Invoke(this));

        input.RegisterCallback<MouseOverEvent>(_ => input.AddClass(hoverClass));
        output.RegisterCallback<MouseOverEvent>(_ => output.AddClass(hoverClass));
        input.RegisterCallback<MouseOutEvent>(_ => input.RemoveClass(hoverClass));
        output.RegisterCallback<MouseOutEvent>(_ => output.RemoveClass(hoverClass));

        this.AddManipulator(new NodeEvents(onPositionChanged));

        RegisterCallback<MouseDownEvent>(evt =>
        {
            if (evt.button == 0)
            {
                evt.StopPropagation();
                OnSelected?.Invoke(this);
            }
        });

        LoadForm();
    }

    private void LoadForm()
    {
        var step = Node.Step;

        var nodeElement = this.Q("node");

        foreach (var className in nodeElement.GetClasses().ToList())
        {
            if (className.StartsWith("node-"))
                nodeElement.RemoveClass(className);
        }

        nodeElement.AddClass("node-" + Node.Step.StepType.ToString().ToLower());

        string stepFormPath = $"{viewsPath}/Steps/{step.GetType().Name}View.uxml";
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(stepFormPath);

        if (visualTree != null)
        {
            var form = visualTree.CloneTree();
            formContainer.Clear();
            formContainer.Add(form);

            var binder = new FormBinder();
            binder.AutoBindFields(form, step);

            if (step is ButtonStep buttonStep)
            {
                var stepFormRoot = form;
                var buttonStepForm = new ButtonStepForm(stepFormRoot);

                buttonStepForm.Setup(buttonStep,
                    onLink: index =>
                    {
                        OnStartLink?.Invoke(this);
                        buttonStep.SelectedButtonIndex = index;
                    },
                    onRemove: index =>
                    {
                        OnRemovedButton?.Invoke(this, index);
                    });
            }
        }
    }

    public void SetPosition(Vector2 pos)
    {
        style.left = pos.x;
        style.top = pos.y;
    }

    public Vector2 GetInputPosition(VisualElement canvas) =>
        canvas.WorldToLocal(input.worldBound.center);

    public Vector2 GetOutputPosition(VisualElement canvas) =>
        canvas.WorldToLocal(output.worldBound.center);

    public void RefreshView()
    {
        title.text = Node.Step.StepType.ToString();

        SetPosition(Node.Position);

        LoadForm();
    }

    public Vector2 GetButtonOutputPosition(int index, VisualElement canvas)
    {
        var step = Node.Step as ButtonStep;
        if (step == null || index < 0 || index >= step.Buttons.Count)
            return GetOutputPosition(canvas);

        var buttonRows = formContainer.Query<VisualElement>(className: "button-row").ToList();
        if (index >= buttonRows.Count)
            return GetOutputPosition(canvas);

        var linkButton = buttonRows[index].Q<Button>("linkButton");
        return canvas.WorldToLocal(linkButton.worldBound.center);
    }

    public void SyncNodeData()
    {
        Node.PositionX = resolvedStyle.left;
        Node.PositionY = resolvedStyle.top;
    }
}
