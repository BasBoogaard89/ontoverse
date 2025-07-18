using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeView : VisualElement
{
    public Action<NodeView> OnStartLink;
    public Action<NodeView> OnCompleteLink;
    public Action<NodeView> OnSelected;

    public DialogueNode Node;

    private VisualElement input, output;
    private Label title;

    public NodeView(DialogueNode node, Action onPositionChanged)
    {
        Node = node;
        Node.Step ??= new TypeStep(EDisplayType.Type);

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/Scripts/UI/Editor/Views/NodeView.uxml"
        );
        visualTree.CloneTree(this);

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
            "Assets/Scripts/UI/Editor/Views/NodeView.uss"
        );
        styleSheets.Add(styleSheet);

        SetPosition(Node.Position);

        input = this.Q("input");
        output = this.Q("output");
        title = this.Q<Label>("title");

        input.RegisterCallback<ClickEvent>(_ => OnCompleteLink?.Invoke(this));
        output.RegisterCallback<ClickEvent>(_ => OnStartLink?.Invoke(this));

        input.RegisterCallback<MouseOverEvent>(_ => input.AddClass("over"));
        output.RegisterCallback<MouseOverEvent>(_ => output.AddClass("over"));
        input.RegisterCallback<MouseOutEvent>(_ => input.RemoveClass("over"));
        output.RegisterCallback<MouseOutEvent>(_ => output.RemoveClass("over"));

        this.AddManipulator(new NodeEvents(onPositionChanged));

        RegisterCallback<MouseDownEvent>(evt =>
        {
            if (evt.button == 0)
            {
                evt.StopPropagation();
                OnSelected?.Invoke(this);
                title.text = Node.Step.StepType.ToString();
            }
        });
    }

    public void SetPosition(Vector2 pos)
    {
        style.left = pos.x;
        style.top = pos.y;
        Node.PositionX = pos.x;
        Node.PositionY = pos.y;
    }

    public Vector2 GetInputPosition(VisualElement canvas) =>
        canvas.WorldToLocal(input.worldBound.center);

    public Vector2 GetOutputPosition(VisualElement canvas) =>
        canvas.WorldToLocal(output.worldBound.center);
}
