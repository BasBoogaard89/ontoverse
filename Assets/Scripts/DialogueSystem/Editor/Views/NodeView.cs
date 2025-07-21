using Ontoverse.Utils;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ontoverse.DialogueSystem.Editor
{
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

        private bool resizing = false;
        private Vector2 resizeStartMouse;
        private Vector2 resizeStartSize;

        public NodeView(DialogueNode node, Action onPositionChanged)
        {
            Node = node;
            Node.Step ??= new TypeStep();
            Node.NextNodeId ??= null;

            var visualTree = AssetHelper.LoadEditorAsset<VisualTreeAsset>("/Views/NodeView.uxml");
            visualTree.CloneTree(this);

            styleSheets.Add(AssetHelper.LoadEditorAsset<StyleSheet>("Views/NodeView.uss"));

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

            var resizer = this.Q("resizer");
            if (resizer != null)
            {
                resizer.RegisterCallback<MouseDownEvent>(OnResizeStart);
            }

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
                } else if (step is ActionStep acs)
                {
                    var actionForm = new ActionStepForm(form);
                    actionForm.Setup(acs);
                } else if (step is TypeStep ts)
                {
                    var typeStepForm = new TypeStepForm(form);
                    typeStepForm.Setup(ts);
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
            Node.Width = resolvedStyle.width;
            Node.Height = resolvedStyle.height;
        }

        private void OnResizeStart(MouseDownEvent evt)
        {
            resizing = true;
            resizeStartMouse = evt.mousePosition;
            resizeStartSize = new Vector2(resolvedStyle.width, resolvedStyle.height);

            this.CaptureMouse();
            RegisterCallback<MouseMoveEvent>(OnResize);
            RegisterCallback<MouseUpEvent>(OnResizeEnd);

            evt.StopPropagation();
        }

        private void OnResize(MouseMoveEvent evt)
        {
            if (!resizing) return;

            var delta = evt.mousePosition - resizeStartMouse;
            float newWidth = resizeStartSize.x + delta.x;
            float newHeight = resizeStartSize.y + delta.y;

            style.width = newWidth;
            style.height = newHeight;

            Node.Width = newWidth;
            Node.Height = newHeight;

            evt.StopPropagation();
            MarkDirtyRepaint();
        }

        private void OnResizeEnd(MouseUpEvent evt)
        {
            resizing = false;
            this.ReleaseMouse();
            UnregisterCallback<MouseMoveEvent>(OnResize);
            UnregisterCallback<MouseUpEvent>(OnResizeEnd);

            evt.StopPropagation();
        }
    }
}
