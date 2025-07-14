using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEditor;
using UnityEngine;

public class DialogueGraphEditor : EditorWindow
{
    private DialogueGraph graph = new();
    private Vector2 offset;
    private Vector2 drag;

    private DialogueNode linkingFromNode = null;
    private Vector2? lastMousePos;

    private Vector2 nodeSize = new Vector2(300, 300);
    private int ButtonsStartY = 340;

    private float lastUpdateTime;
    private const float updateInterval = 1f / 30f;

    private int selectedDialogueIndex = 0;
    private string[] dialoguePaths;
    private string dialogueFolderPath;

    private DialogueNode selectedNode = null;

    [MenuItem("Tools/Dialogue Graph Editor")]
    public static void Open()
    {
        GetWindow<DialogueGraphEditor>("Dialogue Graph Editor");
    }

    private void OnEnable()
    {
        EditorApplication.update += ThrottledUpdate;
        dialogueFolderPath = Application.dataPath + "/Resources/Dialogue";

        if (graph.Nodes.Count == 0)
        {
            string id = Guid.NewGuid().ToString();
            var node = new DialogueNode(id, new Rect(100, 100, nodeSize.x, nodeSize.y));
            graph.Nodes.Add(node);
            graph.EntryNodeId = id;
        }
    }

    private void OnDisable()
    {
        EditorApplication.update -= ThrottledUpdate;
    }

    private void ThrottledUpdate()
    {
        if (EditorApplication.timeSinceStartup - lastUpdateTime > updateInterval)
        {
            lastUpdateTime = (float)EditorApplication.timeSinceStartup;
            Repaint();
        }
    }

    private void OnGUI()
    {
        //DrawGrid(50, 0.4f, Color.gray);

        DrawConnections();
        DrawNodes();
        DrawToolbar();
        DrawLinkLine(Event.current);
        ProcessEvents(Event.current);
    }

    private void DrawToolbar()
    {
        Rect sidebarRect = new Rect(0, 0, 350, position.height);
        EditorGUI.DrawRect(sidebarRect, new Color(0.15f, 0.15f, 0.15f));

        GUIStyle sidebarStyle = new GUIStyle
        {
            padding = new RectOffset(10, 10, 10, 10),
            normal = { background = Texture2D.grayTexture },
            border = new RectOffset(0, 0, 0, 0)
        };

        GUILayout.BeginArea(sidebarRect, GUIContent.none, sidebarStyle);

        GUILayout.Label("Dialogue Files", EditorStyles.boldLabel);

        if (dialoguePaths == null || dialoguePaths.Length == 0)
            LoadDialogueList();

        selectedDialogueIndex = EditorGUILayout.Popup(selectedDialogueIndex, dialoguePaths);

        if (GUILayout.Button("Load"))
        {
            LoadDialogue();
        }

        if (GUILayout.Button("Save As"))
        {
            SaveDialogue();
        }

        GUILayout.Space(10);
        GUILayout.Label("Graph Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Node"))
        {
            var node = new DialogueNode(Guid.NewGuid().ToString(), new Rect(300, 100, nodeSize.x, nodeSize.y));
            graph.Nodes.Add(node);
        }

        GUILayout.Space(100);
        GUILayout.Label("Node Properties", EditorStyles.boldLabel);

        if (selectedNode != null)
        {
            GUILayout.Label("Text:");
            selectedNode.Step.Text = EditorGUILayout.TextArea(selectedNode.Step.Text);

            selectedNode.Step.StepType = (EDialogueStepType)EditorGUILayout.EnumPopup("Action", selectedNode.Step.StepType);
            selectedNode.Step.LogType = (ELogType)EditorGUILayout.EnumPopup("Log Type", selectedNode.Step.LogType);
            selectedNode.Step.Delay = EditorGUILayout.FloatField("Delay", selectedNode.Step.Delay);
            selectedNode.Step.CharacterDelay = EditorGUILayout.FloatField("Char Delay", selectedNode.Step.CharacterDelay);
            selectedNode.Step.ClearBefore = EditorGUILayout.Toggle("Clear Before", selectedNode.Step.ClearBefore);

            GUILayout.Space(10);


            GUILayout.Space(10);
            selectedNode.Step.RequiresUserInput = EditorGUILayout.Toggle("Requires Input", selectedNode.Step.RequiresUserInput);

            if (selectedNode.Step.RequiresUserInput)
            {
                EditorGUILayout.BeginHorizontal();

                bool waitForCommand = selectedNode.Step.WaitForCommand;
                bool showButtons = selectedNode.Step.PresentButtons;

                waitForCommand = EditorGUILayout.ToggleLeft("Wait For Command", waitForCommand);
                showButtons = EditorGUILayout.ToggleLeft("Show Buttons", showButtons);

                if (waitForCommand && showButtons)
                {
                    showButtons = false;
                }

                selectedNode.Step.WaitForCommand = waitForCommand;
                selectedNode.Step.PresentButtons = showButtons;

                EditorGUILayout.EndHorizontal();

                if (selectedNode.Step.WaitForCommand)
                {
                    selectedNode.Step.CommandType = (ECommandType)EditorGUILayout.EnumPopup("Command Type", selectedNode.Step.CommandType);
                }
            }

            if (selectedNode.Step.PresentButtons)
            {
                GUILayout.Label("Buttons:", EditorStyles.boldLabel);
                for (int i = 0; i < selectedNode.Step.Buttons.Count; i++)
                {
                    var btn = selectedNode.Step.Buttons[i];

                    GUILayout.BeginHorizontal();

                    btn.Label = EditorGUILayout.TextArea(btn.Label);

                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        selectedNode.Step.Buttons.RemoveAt(i);
                        break;
                    }

                    if (GUILayout.Button("Out", GUILayout.Width(35)))
                    {
                        linkingFromNode = selectedNode;
                        selectedNode.Step.SelectedButtonIndex = i;
                    }

                    GUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Add Button"))
                {
                    selectedNode.Step.Buttons.Add(new DialogueButtonData($"Option {selectedNode.Step.Buttons.Count}"));
                }
            }
        } else
        {
            GUILayout.Label("No node selected.");
        }

        GUILayout.EndArea();
    }

    private void DrawNodeWindow(DialogueNode node)
    {
        Rect inRect = new Rect(5, 5, 50, 20);
        Rect outRect = new Rect(node.Rect.width - 55, 5, 50, 20);

        if (GUI.Button(inRect, "In"))
        {
            if (linkingFromNode != null && linkingFromNode != node)
            {
                if (linkingFromNode.Step.PresentButtons && linkingFromNode.Step.SelectedButtonIndex >= 0)
                {
                    var index = linkingFromNode.Step.SelectedButtonIndex;
                    if (index >= 0 && index < linkingFromNode.Step.Buttons.Count)
                    {
                        linkingFromNode.Step.Buttons[index].TargetNodeId = node.Id;
                    }
                } else
                {
                    if (!linkingFromNode.NextNodeIds.Contains(node.Id))
                        linkingFromNode.NextNodeIds.Add(node.Id);
                }

                linkingFromNode.Step.SelectedButtonIndex = -1;
                linkingFromNode = null;
            }
        }


        if (GUI.Button(outRect, "Out"))
        {
            linkingFromNode = node;
        }

        GUILayout.BeginHorizontal();

        Rect toggleRect = new Rect(5, 30, 20, 20);
        if (GUI.Button(toggleRect, node.IsCollapsed ? "►" : "▼"))
        {
            node.IsCollapsed = !node.IsCollapsed;
            GUI.changed = true;
        }

        if (node.IsCollapsed)
        {
            Rect labelRect = new Rect(30, 29, node.Rect.width - 35, 20);
            string preview = string.IsNullOrEmpty(node.Step.Text) ? "<Empty>" : node.Step.Text;
            int maxLength = 38;

            if (preview.Length > maxLength)
                preview = preview.Substring(0, maxLength - 3) + "...";

            GUI.Label(labelRect, preview, EditorStyles.label);
        }

        GUILayout.EndHorizontal();

        if (node.IsCollapsed)
        {
            node.Rect.height = 55;
            GUI.DragWindow();
            return;
        }

        GUILayout.Space(30);
        GUILayout.Label("Text:");
        node.Step.Text = EditorGUILayout.TextArea(node.Step.Text);

        GUILayout.Space(5);

        node.Step.StepType = (EDialogueStepType)EditorGUILayout.EnumPopup("Action", node.Step.StepType);
        node.Step.LogType = (ELogType)EditorGUILayout.EnumPopup("Log Type", node.Step.LogType);
        node.Step.Delay = (float)Math.Round(EditorGUILayout.FloatField("Delay", node.Step.Delay), 3);
        node.Step.CharacterDelay = (float)Math.Round(EditorGUILayout.FloatField("Char Delay", node.Step.CharacterDelay), 3);
        node.Step.ClearBefore = EditorGUILayout.Toggle("Clear Before", node.Step.ClearBefore);

        GUILayout.Space(15);
        GUILayout.Label("Interaction:");
        node.Step.RequiresUserInput = EditorGUILayout.Toggle("Requires Input", node.Step.RequiresUserInput);
        node.Step.WaitForCommand = EditorGUILayout.Toggle("Wait For Command", node.Step.WaitForCommand);
        node.Step.CommandType = (ECommandType)EditorGUILayout.EnumPopup("Command Type", node.Step.CommandType);
        node.Step.PresentButtons = EditorGUILayout.Toggle("Show Buttons", node.Step.PresentButtons);

        if (node.Step.PresentButtons)
        {
            GUILayout.Label("Buttons:", EditorStyles.boldLabel);
            for (int i = 0; i < node.Step.Buttons.Count; i++)
            {
                var btn = node.Step.Buttons[i];

                GUILayout.BeginHorizontal();

                btn.Label = EditorGUILayout.TextArea(btn.Label);

                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    node.Step.Buttons.RemoveAt(i);
                    break;
                }

                if (GUILayout.Button("Out", GUILayout.Width(35)))
                {
                    linkingFromNode = node;
                    node.Step.SelectedButtonIndex = i;
                }

                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Button"))
            {
                node.Step.Buttons.Add(new DialogueButtonData($"Option {node.Step.Buttons.Count}"));
            }
        }

        GUI.DragWindow();

        float contentHeight = GUILayoutUtility.GetLastRect().yMax + 40;
        node.Rect.height = Mathf.Max(contentHeight, 500);
    }

    private void ProcessEvents(Event e)
    {
        drag = Vector2.zero;

        switch (e.type)
        {
            case EventType.KeyDown:
                if (e.keyCode == KeyCode.Escape)
                {
                    linkingFromNode = null;
                    lastMousePos = null;
                    GUI.changed = true;
                    e.Use();
                }
                break;

            case EventType.MouseDown:
                if (e.button == 0)
                {
                    bool clickedOnNode = false;

                    foreach (var node in graph.Nodes)
                    {
                        if (node.Rect.Contains(e.mousePosition))
                        {
                            selectedNode = node;
                            clickedOnNode = true;
                            Debug.Log("Node changed!");
                            Debug.Log(selectedNode.Step.Text);
                            GUI.changed = true;
                            break;
                        }
                    }

                    if (linkingFromNode != null && !clickedOnNode)
                    {
                        Vector2 spawnPos = e.mousePosition;

                        var newNode = new DialogueNode(Guid.NewGuid().ToString(), new Rect(spawnPos.x, spawnPos.y, nodeSize.x, nodeSize.y));
                        graph.Nodes.Add(newNode);

                        if (linkingFromNode.Step.PresentButtons && linkingFromNode.Step.SelectedButtonIndex >= 0)
                        {
                            var btn = linkingFromNode.Step.Buttons[linkingFromNode.Step.SelectedButtonIndex];
                            btn.TargetNodeId = newNode.Id;
                        } else
                        {
                            linkingFromNode.NextNodeIds.Add(newNode.Id);
                        }

                        linkingFromNode.Step.SelectedButtonIndex = -1;
                        linkingFromNode = null;

                        GUI.changed = true;
                    }

                    if (!clickedOnNode)
                        selectedNode = null;

                    e.Use();
                }
                break;

            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    foreach (var node in graph.Nodes)
                    {
                        if (node.Rect.Contains(e.mousePosition))
                        {
                            node.Drag(e.delta);
                            GUI.changed = true;
                            return;
                        }
                    }
                }

                if (e.button == 2)
                {
                    OnDrag(e.delta);
                }
                break;
        }
    }

    private void LoadDialogueList()
    {
        if (!Directory.Exists(dialogueFolderPath))
            Directory.CreateDirectory(dialogueFolderPath);

        var files = Directory.GetFiles(dialogueFolderPath, "*.json");
        dialoguePaths = new string[files.Length];
        for (int i = 0; i < files.Length; i++)
            dialoguePaths[i] = Path.GetFileName(files[i]);
    }

    private void LoadDialogue()
    {
        var decode = false;

        string path = dialogueFolderPath + "/" + dialoguePaths[selectedDialogueIndex];
        if (File.Exists(path))
        {
            string json;

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(dialoguePaths[selectedDialogueIndex]);

            TextAsset fileData = Resources.Load<TextAsset>("Dialogue/" + fileNameWithoutExtension);

            if (decode)
            {
                json = DecompressBytes(fileData.bytes);
            } else
            {
                json = File.ReadAllText(path);
            }

            graph = JsonUtility.FromJson<DialogueGraph>(json);
            HandleNodesAfterLoading();
        }
    }

    private void SaveDialogue()
    {
        var encode = false;

        string path = EditorUtility.SaveFilePanel("Save Terminal Graph", dialogueFolderPath, dialoguePaths.Length > 0 ? Path.GetFileNameWithoutExtension(dialoguePaths[selectedDialogueIndex]) : "empty", "json");
        if (!string.IsNullOrEmpty(path))
        {
            PrepareNodesBeforeSaving();

            string json = JsonUtility.ToJson(graph, false);

            if (encode)
            {
                byte[] compressed = CompressString(json);
                File.WriteAllBytes(path, compressed);
            } else
            {
                json = JsonUtility.ToJson(graph);
                File.WriteAllText(path, json);
            }

            Debug.Log("Graph saved to " + path);

            AssetDatabase.Refresh();

            LoadDialogueList();
        }
    }

    private void DrawNodes()
    {
        BeginWindows();
        for (int i = 0; i < graph.Nodes.Count; i++)
        {
            var node = graph.Nodes[i];
            node.Rect = GUI.Window(i, node.Rect, id => DrawNodeWindow(node), $"Node {node.Id.Substring(0, 6)}");

        }
        EndWindows();
    }

    private void DrawConnections()
    {
        foreach (var node in graph.Nodes)
        {
            foreach (var nextId in node.NextNodeIds)
            {
                var target = graph.GetNodeById(nextId);
                if (target != null)
                {
                    Vector2 start = new Vector2(node.Rect.xMax - 5, node.Rect.yMin + 15);
                    Vector2 end = new Vector2(target.Rect.xMin + 5, target.Rect.yMin + 15);

                    Handles.DrawBezier(
                        start,
                        end,
                        start + Vector2.right * 50,
                        end + Vector2.left * 50,
                        Color.white,
                        null,
                        2f);
                }
            }

            for (int i = 0; i < node.Step.Buttons.Count; i++)
            {
                var btn = node.Step.Buttons[i];
                if (!string.IsNullOrEmpty(btn.TargetNodeId))
                {
                    var target = graph.GetNodeById(btn.TargetNodeId);
                    if (target != null)
                    {
                        Vector2 start;

                        if (node.IsCollapsed)
                        {
                            float outY = node.Rect.yMin + 40;
                            start = new Vector2(node.Rect.xMax - 5, outY);
                        } else
                        {
                            float buttonY = node.Rect.yMin + ButtonsStartY + (i * 20);
                            start = new Vector2(node.Rect.xMax - 5, buttonY);
                        }

                        Vector2 end = new Vector2(target.Rect.xMin + 5, target.Rect.yMin + 15);

                        Handles.DrawBezier(
                            start,
                            end,
                            start + Vector2.right * 50,
                            end + Vector2.left * 50,
                            Color.cyan,
                            null,
                            2f);
                    }
                }
            }

        }
    }

    private void DrawLinkLine(Event e)
    {
        if (linkingFromNode != null && !string.IsNullOrEmpty(linkingFromNode.Id))
        {
            if (lastMousePos == null || lastMousePos.Value != e.mousePosition)
            {
                lastMousePos = e.mousePosition;
                GUI.changed = true;
            }

            Vector2 start;

            if (linkingFromNode.Step.PresentButtons && linkingFromNode.Step.SelectedButtonIndex >= 0)
            {
                float buttonY = linkingFromNode.Rect.yMin + ButtonsStartY + (linkingFromNode.Step.SelectedButtonIndex * 20);
                start = new Vector2(linkingFromNode.Rect.xMax - 5, buttonY);
            } else
            {
                start = new Vector2(linkingFromNode.Rect.xMax - 5, linkingFromNode.Rect.yMin + 15);
            }

            Handles.DrawBezier(
                start,
                lastMousePos.Value,
                start + Vector2.right * 50,
                lastMousePos.Value + Vector2.left * 50,
                Color.yellow,
                null,
                2f);
        }
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset,
                             new Vector3(gridSpacing * i, position.height, 0f) + newOffset);

        for (int j = 0; j < heightDivs; j++)
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset,
                             new Vector3(position.width, gridSpacing * j, 0f) + newOffset);

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    void OnDrag(Vector2 delta)
    {
        drag = delta;

        foreach (var node in graph.Nodes)
        {
            node.Drag(delta);
        }

        GUI.changed = true;
    }

    void PrepareNodesBeforeSaving()
    {
    }

    void HandleNodesAfterLoading()
    {
        foreach (var node in graph.Nodes)
        {
            node.Rect = new Rect(node.Position, nodeSize);
        }
    }

    static byte[] CompressString(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, System.IO.Compression.CompressionLevel.Optimal))
        {
            gzip.Write(bytes, 0, bytes.Length);
        }
        return output.ToArray();
    }

    static string DecompressBytes(byte[] compressed)
    {
        using var input = new MemoryStream(compressed);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip);
        return reader.ReadToEnd();
    }
}
