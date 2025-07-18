using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueEditor : EditorWindow
{
    private VisualElement root, canvas;
    private DropdownField dialogueDropdown;
    private Button saveDialogueButton;
    private NodeGraphService graphService;

    private string[] dialoguePaths;
    private string currentDialogueFilename;

    [MenuItem("Tools/Dialogue Editor")]
    public static void ShowExample()
    {
        DialogueEditor wnd = GetWindow<DialogueEditor>();
        wnd.titleContent = new GUIContent("Dialogue Editor");
    }

    public void CreateGUI()
    {
        CreateWindow();
        BindElementsAndControls();

        graphService = new NodeGraphService(canvas, () => hasUnsavedChanges = true);

        LoadDialogueList();
    }

    void CreateWindow()
    {
        root = rootVisualElement;
        VisualTreeAsset visualTreeAsset = AssetHelper.LoadAsset<VisualTreeAsset>("/DialogueEditor.uxml");

        var uxmlRoot = visualTreeAsset.CloneTree();

        root.Add(uxmlRoot);

        var styleSheet = AssetHelper.LoadAsset<StyleSheet>("/DialogueEditor.uss");

        if (styleSheet != null)
            root.styleSheets.Add(styleSheet);
    }

    void BindElementsAndControls()
    {
        canvas = root.Q<VisualElement>("canvas");
        dialogueDropdown = root.Q<DropdownField>("dialogueDropdown");
        saveDialogueButton = root.Q<Button>("saveButton");

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
        var graph = DialogueFileService.LoadDialogue(filename);
        if (graph != null)
            graphService.LoadGraph(graph);
    }

    private void SaveDialogue()
    {
        string defaultName = dialoguePaths.Length > 0
            ? Path.GetFileNameWithoutExtension(dialoguePaths[dialogueDropdown.index])
            : "dialogue";

        var graph = graphService.GetCurrentGraph();
        DialogueFileService.SaveDialogue(graph, defaultName);

        LoadDialogueList();

        hasUnsavedChanges = false;
    }
}