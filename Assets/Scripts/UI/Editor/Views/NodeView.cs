using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeView : VisualElement
{
    public Action<NodeView> OnStartLink;
    public Action<NodeView> OnCompleteLink;
    public Action<NodeView> OnSelected;

    public DialogueNode Model;

    private VisualElement input;
    private VisualElement output;

    public NodeView(DialogueNode model, Action onPositionChanged)
    {
        Model = model;
        Model.Step ??= new DialogueStep(EDialogueStepType.None);

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/Scripts/UI/Editor/Views/NodeView.uxml"
        );
        visualTree.CloneTree(this);

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
            "Assets/Scripts/UI/Editor/Views/NodeView.uss"
        );
        styleSheets.Add(styleSheet);

        SetPosition(Model.Position);

        input = this.Q<Button>("input");
        output = this.Q<Button>("output");

        input.RegisterCallback<ClickEvent>(_ => OnCompleteLink?.Invoke(this));
        output.RegisterCallback<ClickEvent>(_ => OnStartLink?.Invoke(this));

        input.RegisterCallback<MouseOverEvent>(_ => input.AddClass("over"));
        output.RegisterCallback<MouseOverEvent>(_ => output.AddClass("over"));
        input.RegisterCallback<MouseOutEvent>(_ => input.RemoveClass("over"));
        output.RegisterCallback<MouseOutEvent>(_ => output.RemoveClass("over"));

        this.AddManipulator(new NodeDragManipulator(onPositionChanged));

        RegisterCallback<MouseDownEvent>(evt =>
        {
            if (evt.button == 0)
            {
                evt.StopPropagation();
                OnSelected?.Invoke(this);
            }
        });
    }

    public void SetPosition(Vector2 pos)
    {
        style.left = pos.x;
        style.top = pos.y;
        Model.PositionX = pos.x;
        Model.PositionY = pos.y;
    }

    public Vector2 GetInputPosition(VisualElement canvas) =>
        canvas.WorldToLocal(input.worldBound.center);

    public Vector2 GetOutputPosition(VisualElement canvas) =>
        canvas.WorldToLocal(output.worldBound.center);
}
