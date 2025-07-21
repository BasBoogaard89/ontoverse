using Ontoverse.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ontoverse.DialogueSystem.Editor
{
    public class NodeGraphService
    {
        public readonly List<NodeView> nodes = new();
        public List<(NodeView from, NodeView to, int? buttonIndex)> connections = new();
        public NodeView linkingFrom;
        public Vector2 currentMousePosition;
        private Vector2 lastPanMousePosition;

        private readonly VisualElement canvas;
        private readonly Action onPositionChanged;
        private bool isPanning = false;
        private DialogueGraph activeGraph;

        public NodeGraphService(VisualElement canvas, Action onPositionChanged)
        {
            this.canvas = canvas;
            this.onPositionChanged = onPositionChanged;
        }

        public void LoadGraph(DialogueGraph graph)
        {
            Clear();
            activeGraph = graph;

            var idToNodeView = AddNodeViews(graph);
            AddNodeConnections(graph, idToNodeView);

            if (!string.IsNullOrEmpty(graph.EntryNodeId) && idToNodeView.TryGetValue(graph.EntryNodeId, out var entryView))
            {
                SetEntryNode(entryView);
            }

            canvas.MarkDirtyRepaint();
        }

        private Dictionary<string, NodeView> AddNodeViews(DialogueGraph graph)
        {
            var idToNodeView = new Dictionary<string, NodeView>();

            foreach (var nodeData in graph.Nodes)
            {
                var view = AddNodeView(nodeData);
                idToNodeView[nodeData.Id] = view;
            }

            return idToNodeView;
        }

        private void AddNodeConnections(DialogueGraph graph, Dictionary<string, NodeView> idToNodeView)
        {
            foreach (var nodeData in graph.Nodes)
            {
                if (!idToNodeView.TryGetValue(nodeData.Id, out var fromView))
                    continue;

                if (!string.IsNullOrEmpty(nodeData.NextNodeId) &&
                    idToNodeView.TryGetValue(nodeData.NextNodeId, out var toView))
                {
                    connections.Add((fromView, toView, null));
                }

                if (nodeData.Step is ButtonStep btnStep)
                {
                    for (int i = 0; i < btnStep.Buttons.Count; i++)
                    {
                        var btn = btnStep.Buttons[i];
                        if (!string.IsNullOrEmpty(btn.TargetNodeId) &&
                            idToNodeView.TryGetValue(btn.TargetNodeId, out var toBtnView))
                        {
                            connections.Add((fromView, toBtnView, i));
                        }
                    }
                }
            }
        }

        public NodeView AddNodeView(DialogueNode node)
        {
            var view = new NodeView(node, onPositionChanged);

            view.OnStartLink += StartLink;
            view.OnCompleteLink += TryCompleteLink;
            view.OnRemovedNode += RemoveNode;
            view.OnRemovedButton += RemoveButtonFromNode;
            view.OnMarkEntryNode += SetEntryNode;

            view.RefreshView();

            nodes.Add(view);
            canvas.Add(view);

            canvas.MarkDirtyRepaint();

            return view;
        }

        public NodeView AddNodeView(Vector2 position)
        {
            var node = new DialogueNode
            {
                Id = Guid.NewGuid().ToString(),
                PositionX = position.x,
                PositionY = position.y,
            };
            var view = AddNodeView(node);

            return view;
        }

        public NodeView AddNodeView(Vector2 position, BaseStep step)
        {
            var view = AddNodeView(position);
            view.Node.Step = step;

            view.RefreshView();

            return view;
        }

        public void DrawConnections(Painter2D painter)
        {
            painter.strokeColor = Color.cyan;
            painter.lineWidth = 2;

            foreach (var (from, to, buttonIndex) in connections)
            {
                Vector2 start = buttonIndex.HasValue
                    ? from.GetButtonOutputPosition(buttonIndex.Value, canvas)
                    : from.GetOutputPosition(canvas);

                Vector2 end = to.GetInputPosition(canvas);

                DrawBezier(painter, start, end);
            }

            if (linkingFrom != null)
            {
                Vector2 start;

                if (linkingFrom.Node.Step is ButtonStep btnStep && btnStep.SelectedButtonIndex >= 0)
                    start = linkingFrom.GetButtonOutputPosition(btnStep.SelectedButtonIndex, canvas);
                else
                    start = linkingFrom.GetOutputPosition(canvas);

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
            if (linkingFrom == null || linkingFrom == target)
                return;

            var fromStep = linkingFrom.Node.Step;

            int? outputIndex = null;

            if (fromStep is ButtonStep buttonStep && buttonStep.SelectedButtonIndex >= 0)
            {
                outputIndex = buttonStep.SelectedButtonIndex;

                buttonStep.Buttons[outputIndex.Value].TargetNodeId = target.Node.Id;

                buttonStep.SelectedButtonIndex = -1;
            } else
            {
                linkingFrom.Node.NextNodeId = target.Node.Id;
            }

            connections.RemoveAll(c =>
                c.from == linkingFrom &&
                c.buttonIndex == outputIndex);

            connections.Add((linkingFrom, target, outputIndex));

            linkingFrom = null;
            canvas.MarkDirtyRepaint();
            onPositionChanged?.Invoke();
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

            if (evt.button == 1)
            {
                Vector2 clickPos = canvas.WorldToLocal(evt.originalMousePosition);
                ShowStepSelectionMenu(clickPos);
                return;
            }

            if (linkingFrom != null && evt.button == 0)
            {
                Vector2 releasePos = canvas.WorldToLocal(evt.originalMousePosition);
                ShowStepSelectionMenu(releasePos);
            }
        }

        public void ShowStepSelectionMenu(Vector2 position)
        {
            var menu = new GenericMenu();

            foreach (var kv in StepFactory.Step)
            {
                string stepName = kv.Key.ToString();
                menu.AddItem(new GUIContent(stepName), false, () =>
                {
                    var step = kv.Value.Invoke();
                    var node = AddNodeView(position, step);

                    if (linkingFrom != null)
                    {
                        int? buttonIndex = null;

                        if (linkingFrom.Node.Step is ButtonStep btnStep && btnStep.SelectedButtonIndex >= 0)
                        {
                            btnStep.Buttons[btnStep.SelectedButtonIndex].TargetNodeId = node.Node.Id;
                            buttonIndex = btnStep.SelectedButtonIndex;
                            btnStep.SelectedButtonIndex = -1;
                        } else
                        {
                            linkingFrom.Node.NextNodeId = node.Node.Id;
                        }

                        connections.Add((linkingFrom, node, buttonIndex));
                        linkingFrom = null;
                        onPositionChanged?.Invoke();
                    }

                    canvas.MarkDirtyRepaint();
                });
            }

            menu.ShowAsContext();
        }

        public void RemoveNode(NodeView nodeView)
        {
            if (!nodes.Contains(nodeView))
                return;

            connections.RemoveAll(conn =>
                conn.from == nodeView || conn.to == nodeView);

            foreach (var view in nodes)
            {
                if (view.Node.Step is ButtonStep btnStep)
                {
                    foreach (var btn in btnStep.Buttons)
                    {
                        if (btn.TargetNodeId == nodeView.Node.Id)
                            btn.TargetNodeId = null;
                    }
                }

                if (view.Node.NextNodeId == nodeView.Node.Id)
                    view.Node.NextNodeId = null;
            }

            canvas.Remove(nodeView);
            nodes.Remove(nodeView);

            canvas.MarkDirtyRepaint();
            onPositionChanged?.Invoke();
        }

        private void RemoveButtonFromNode(NodeView nodeView, int indexToRemove)
        {
            if (nodeView.Node.Step is not ButtonStep buttonStep)
                return;

            if (indexToRemove < 0 || indexToRemove >= buttonStep.Buttons.Count)
                return;

            buttonStep.SelectedButtonIndex = -1;
            buttonStep.Buttons.RemoveAt(indexToRemove);

            connections = connections
                .Where(c =>
                    !(c.from == nodeView && c.buttonIndex == indexToRemove))
                .Select(c =>
                {
                    if (c.from == nodeView && c.buttonIndex.HasValue && c.buttonIndex > indexToRemove)
                        return (c.from, c.to, c.buttonIndex - 1);
                    return c;
                })
                .ToList();

            nodeView.RefreshView();
            canvas.MarkDirtyRepaint();
            onPositionChanged?.Invoke();
        }

        public void SetEntryNode(NodeView entryNode)
        {
            if (activeGraph == null)
                return;

            activeGraph.EntryNodeId = entryNode.Node.Id;

            foreach (var node in nodes)
            {
                if (entryNode.Node.Id == node.Node.Id)
                    node.Q("node").AddClass("entry-node");
                else
                    node.Q("node").RemoveClass("entry-node");
            }
            canvas.MarkDirtyRepaint();
        }

        public DialogueGraph ExportGraph()
        {
            foreach (var node in nodes)
                node.SyncNodeData();

            return new DialogueGraph
            {
                EntryNodeId = activeGraph.EntryNodeId,
                Nodes = nodes.Select(n => n.Node).ToList()
            };
        }
    }
}
