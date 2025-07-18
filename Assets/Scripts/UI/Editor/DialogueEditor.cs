using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueEditor : EditorWindow
{
    private VisualElement root, sidebar, canvas;
    private DropdownField dialogueDropdown;
    private Button saveDialogueButton, addNodeButton;
    private Label noSelectionLabel;
    private VisualElement nodeForm, inputOptions, buttonsList;
    private Button addButton;
    private Toggle requiresInputToggle, waitForCommandToggle, presentButtonsToggle;

    private TextField nodeTextField;
    private EnumField stepTypeField, logTypeField, commandTypeField;
    private FloatField delayField, charDelayField;
    private Toggle clearBeforeToggle;

    private ToolbarFormBinder binder;
    private NodeView selectedNode;

    private NodeGraphService graphService;

    private string[] dialoguePaths;
    private string currentDialogueFilename;

    [MenuItem("Tools/DialogueEditorV2")]
    public static void ShowExample()
    {
        DialogueEditor wnd = GetWindow<DialogueEditor>();
        wnd.titleContent = new GUIContent("DialogueEditor");
    }

    public void CreateGUI()
    {
        CreateWindow();
        BindElementsAndControls();
        RegisterInteraction();

        graphService = new NodeGraphService(canvas, SelectNode, () => hasUnsavedChanges = true);

        LoadDialogueList();
    }

    void CreateWindow()
    {
        root = rootVisualElement;
        VisualTreeAsset visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/Scripts/UI/Editor/DialogueEditor.uxml"
        );

        var uxmlRoot = visualTreeAsset.CloneTree();

        root.Add(uxmlRoot);

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
            "Assets/Scripts/UI/Editor/DialogueEditor.uss"
        );

        if (styleSheet != null)
            root.styleSheets.Add(styleSheet);
    }

    void BindElementsAndControls()
    {
        sidebar = root.Q<VisualElement>("sidebar");
        canvas = root.Q<VisualElement>("canvas");

        dialogueDropdown = root.Q<DropdownField>("dialogueDropdown");
        saveDialogueButton = root.Q<Button>("saveButton");
        addNodeButton = root.Q<Button>("addNodeButton");

        noSelectionLabel = root.Q<Label>("noSelectionLabel");
        nodeForm = root.Q<VisualElement>("nodeForm");
        inputOptions = root.Q<VisualElement>("inputOptions");
        commandTypeField = root.Q<EnumField>("commandTypeField");
        buttonsList = root.Q<VisualElement>("buttonsList");
        addButton = root.Q<Button>("addButton");

        requiresInputToggle = root.Q<Toggle>("requiresInputToggle");
        waitForCommandToggle = root.Q<Toggle>("waitForCommandToggle");
        presentButtonsToggle = root.Q<Toggle>("presentButtonsToggle");

        nodeTextField = root.Q<TextField>("nodeText");
        stepTypeField = root.Q<EnumField>("stepType");
        logTypeField = root.Q<EnumField>("logType");
        delayField = root.Q<FloatField>("delayField");
        charDelayField = root.Q<FloatField>("charDelayField");
        clearBeforeToggle = root.Q<Toggle>("clearBeforeToggle");
    }

    void RegisterInteraction()
    {
        // Nodes interaction
        canvas.generateVisualContent += ctx => graphService.DrawConnections(ctx.painter2D);

        canvas.RegisterCallback<MouseDownEvent>(evt => graphService.OnMouseDown(evt));
        canvas.RegisterCallback<MouseMoveEvent>(evt => graphService.OnMouseMove(evt));
        canvas.RegisterCallback<MouseUpEvent>(evt => graphService.OnMouseUp(evt));

        // Sidebar controls
        dialogueDropdown.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue != currentDialogueFilename)
            {
                TrySwitchDialogue(evt.newValue, revert =>
                {
                    if (revert)
                        dialogueDropdown.SetValueWithoutNotify(currentDialogueFilename);
                });
            }
        });

        saveDialogueButton.clicked += SaveDialogue;
        addNodeButton.clicked += () => graphService.AddNode(new Vector2(100 + graphService.nodes.Count * 30, 100));
    }

    private void TrySwitchDialogue(string newValue, Action<bool> onDecision)
    {
        if (hasUnsavedChanges)
        {
            bool confirm = EditorUtility.DisplayDialog(
                "Niet opgeslagen wijzigingen",
                "Je hebt niet opgeslagen wijzigingen. Weet je zeker dat je wil wisselen?",
                "Ja, wissel",
                "Nee, blijf hier"
            );

            if (!confirm)
            {
                onDecision?.Invoke(true);
                return;
            }
        }

        currentDialogueFilename = newValue;
        LoadDialogue(newValue);
        hasUnsavedChanges = false;
        onDecision?.Invoke(false);
    }

    void SelectNode(NodeView node)
    {
        binder?.UnbindAll();

        selectedNode = node;

        noSelectionLabel.SetDisplay(false);
        nodeForm.SetDisplay(true);
        inputOptions.SetDisplay(false);
        buttonsList.Clear();
        addButton.SetDisplay(false);

        addButton.clicked -= AddButtonToSelected;
        commandTypeField.UnregisterValueChangedCallback(UpdateCommandType);

        var step = node.Model.Step;
        binder = new ToolbarFormBinder(
            () => SelectNode(node),
            () => hasUnsavedChanges = true
        );

        binder.Bind(nodeTextField, () => step.Text, v => step.Text = v);
        binder.Bind(stepTypeField, () => step.StepType, v => step.StepType = v);
        binder.Bind(logTypeField, () => step.LogType, v => step.LogType = v);
        binder.Bind(delayField, () => step.Delay, v => step.Delay = v);
        binder.Bind(charDelayField, () => step.CharacterDelay, v => step.CharacterDelay = v);
        binder.Bind(clearBeforeToggle, () => step.ClearBefore, v => step.ClearBefore = v);
        binder.Bind(requiresInputToggle, () => step.RequiresUserInput, v => step.RequiresUserInput = v, inputOptions);
        binder.Bind(waitForCommandToggle, () => step.WaitForCommand, v => step.WaitForCommand = v, commandTypeField);
        binder.Bind(presentButtonsToggle, () => step.PresentButtons, v => step.PresentButtons = v, buttonsList);
        binder.Bind(commandTypeField, () => step.CommandType, v => step.CommandType = v);

        // Buttons
        buttonsList.Clear();
        if (step.PresentButtons)
        {
            for (int i = 0; i < step.Buttons.Count; i++)
            {
                int index = i;
                var btn = step.Buttons[i];

                var container = new VisualElement { style = { flexDirection = FlexDirection.Row } };

                var labelField = new TextField { value = btn.Label };
                labelField.RegisterValueChangedCallback(evt => {
                    btn.Label = evt.newValue;
                    hasUnsavedChanges = true;
                });
                container.Add(labelField);

                var removeBtn = new Button(() =>
                {
                    step.Buttons.RemoveAt(index);
                    SelectNode(node);
                    hasUnsavedChanges = true;
                })
                { text = "X" };
                container.Add(removeBtn);

                var linkBtn = new Button(() =>
                {
                    graphService.linkingFrom = node;
                    node.Model.Step.SelectedButtonIndex = index;
                })
                { text = "Out" };
                container.Add(linkBtn);

                buttonsList.Add(container);
            }

            addButton.SetDisplay(true);
        } else
        {
            addButton.SetDisplay(false);
        }

        addButton.clicked -= AddButtonToSelected;
        addButton.clicked += AddButtonToSelected;
    }

    void UpdateCommandType(ChangeEvent<Enum> evt)
    {
        if (selectedNode?.Model.Step != null)
            selectedNode.Model.Step.CommandType = (ECommandType)evt.newValue;
    }

    void AddButtonToSelected()
    {
        if (selectedNode?.Model.Step != null)
        {
            selectedNode.Model.Step.Buttons.Add(new DialogueButtonData($"Option {selectedNode.Model.Step.Buttons.Count}"));
            SelectNode(selectedNode);

            hasUnsavedChanges = true;
        }
    }

    void LoadDialogueList()
    {
        dialoguePaths = DialogueFileService.GetAllFilenames();
        dialogueDropdown.choices = new List<string>(dialoguePaths);

        if (dialoguePaths.Length > 0)
        {
            dialogueDropdown.index = 0;
            LoadDialogue(dialoguePaths[0]);
        }
    }

    void LoadDialogue(string filename)
    {
        DeselectNode();

        var graph = DialogueFileService.LoadDialogue(filename);
        if (graph != null)
            graphService.LoadGraph(graph);
    }

    private void SaveDialogue()
    {
        string defaultName = dialoguePaths.Length > 0
            ? Path.GetFileNameWithoutExtension(dialoguePaths[dialogueDropdown.index])
            : "dialogue";

        var graph = new DialogueGraph { Nodes = graphService.Export() };
        DialogueFileService.SaveDialogue(graph, defaultName);

        LoadDialogueList();

        hasUnsavedChanges = false;
    }

    void DeselectNode()
    {
        selectedNode = null;
        binder?.UnbindAll();
        binder = null;

        noSelectionLabel.SetDisplay(true);
        nodeForm.SetDisplay(false);
        inputOptions.SetDisplay(false);
        buttonsList.Clear();
        addButton.SetDisplay(false);
    }

}