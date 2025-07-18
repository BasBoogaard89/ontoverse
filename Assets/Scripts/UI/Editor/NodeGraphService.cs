using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeGraphService
{
    public readonly List<NodeView> nodes = new();
    public readonly List<(NodeView from, NodeView to)> connections = new();
    public NodeView linkingFrom;
    public Vector2 currentMousePosition;
    private Vector2 lastPanMousePosition;

    private readonly VisualElement canvas;
    private readonly Action<NodeView> onSelectNode;
    private readonly Action onPositionChanged;
    private bool isPanning = false;

    public NodeGraphService(VisualElement canvas, Action<NodeView> onSelectNode, Action onPositionChanged)
    {
        this.canvas = canvas;
        this.onSelectNode = onSelectNode;
        this.onPositionChanged = onPositionChanged;
    }

    public void LoadGraph(DialogueGraph graph)
    {
        Clear();

        var idToNodeView = new Dictionary<string, NodeView>();

        foreach (var nodeData in graph.Nodes)
        {
            var view = AddNode(nodeData.Position);
            view.Model = nodeData;
            idToNodeView[nodeData.Id] = view;
        }

        foreach (var nodeData in graph.Nodes)
        {
            if (idToNodeView.TryGetValue(nodeData.Id, out var fromView))
            {
                foreach (var targetId in nodeData.NextNodeIds)
                {
                    if (idToNodeView.TryGetValue(targetId, out var toView))
                    {
                        connections.Add((fromView, toView));
                    }
                }
            }
        }

        canvas.MarkDirtyRepaint();
    }

    public NodeView AddNode(Vector2 position)
    {
        var nodeModel = new DialogueNode();
        var view = new NodeView(nodeModel, onPositionChanged);

        view.OnStartLink += StartLink;
        view.OnCompleteLink += TryCompleteLink;
        view.OnSelected += onSelectNode;

        nodes.Add(view);
        canvas.Add(view);
        view.SetPosition(position);

        return view;
    }

    public void DrawConnections(Painter2D painter)
    {
        painter.strokeColor = Color.cyan;
        painter.lineWidth = 2;

        foreach (var (from, to) in connections)
        {
            DrawBezier(painter, from.GetOutputPosition(canvas), to.GetInputPosition(canvas));
        }

        if (linkingFrom != null)
        {
            Vector2 start = linkingFrom.GetOutputPosition(canvas);
            Vector2 end = currentMousePosition;
            DrawBezier(painter, start, end);
        }
    }

    private void DrawBezier(Painter2D painter, Vector2 start, Vector2 end)
    {
        Vector2 startTangent = start + Vector2.right * 50;
        Vector2 endTangent = end + Vector2.left * 50;

        painter.BeginPath();
        painter.MoveTo(start);
        painter.BezierCurveTo(startTangent, endTangent, end);
        painter.Stroke();
    }

    private void StartLink(NodeView from)
    {
        linkingFrom = from;
    }

    private void TryCompleteLink(NodeView target)
    {
        if (linkingFrom != null && linkingFrom != target)
        {
            connections.Add((linkingFrom, target));
            linkingFrom = null;
            canvas.MarkDirtyRepaint();
            onPositionChanged?.Invoke();
        }
    }

    public List<DialogueNode> Export()
    {
        return nodes.Select(n => n.Model).ToList();
    }

    public void Clear()
    {
        foreach (var n in nodes)
            canvas.Remove(n);
        nodes.Clear();
        connections.Clear();
    }

    public void OnMouseDown(MouseDownEvent evt)
    {
        if (evt.button == 2)
        {
            isPanning = true;
            lastPanMousePosition = evt.mousePosition;
            canvas.CaptureMouse();
        }
    }

    public void OnMouseMove(MouseMoveEvent evt)
    {
        if (linkingFrom != null)
        {
            currentMousePosition = canvas.WorldToLocal(evt.originalMousePosition);
            canvas.MarkDirtyRepaint();
        }

        if (isPanning)
        {
            Vector2 delta = evt.mousePosition - lastPanMousePosition;
            lastPanMousePosition = evt.mousePosition;

            foreach (var child in canvas.Children())
            {
                child.style.left = child.resolvedStyle.left + delta.x;
                child.style.top = child.resolvedStyle.top + delta.y;
            }

            canvas.MarkDirtyRepaint();
        }
    }

    public void OnMouseUp(MouseUpEvent evt)
    {
        if (evt.button == 2 && isPanning)
        {
            isPanning = false;
            canvas.ReleaseMouse();
        }
    }

}
