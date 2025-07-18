using System;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeEvents : PointerManipulator
{
    private bool dragging;
    private readonly Action onDragComplete;

    public NodeEvents(Action onDragComplete)
    {
        this.onDragComplete = onDragComplete;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        target.RegisterCallback<MouseDownEvent>(_ => target.BringToFront());
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        target.UnregisterCallback<MouseDownEvent>(_ => target.BringToFront());
    }

    void OnPointerDown(PointerDownEvent evt)
    {
        dragging = true;
        target.CapturePointer(evt.pointerId);
    }

    void OnPointerMove(PointerMoveEvent evt)
    {
        if (!dragging) return;

        float newLeft = target.resolvedStyle.left + evt.deltaPosition.x;
        float newTop = target.resolvedStyle.top + evt.deltaPosition.y;

        target.style.left = newLeft;
        target.style.top = newTop;

        if (target.parent is VisualElement canvas)
            canvas.MarkDirtyRepaint();
    }

    void OnPointerUp(PointerUpEvent evt)
    {
        dragging = false;
        target.ReleasePointer(evt.pointerId);

        if (target is NodeView node)
        {
            Vector2 finalPos = new Vector2(node.resolvedStyle.left, node.resolvedStyle.top);
            node.SetPosition(finalPos);
            onDragComplete?.Invoke();
        }
    }
}