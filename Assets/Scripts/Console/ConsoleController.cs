using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ConsoleTyper))]
[RequireComponent(typeof(ConsoleInputManager))]
[RequireComponent(typeof(ConsoleButtonManager))]
public class ConsoleController : MonoBehaviour
{
    [JsonDialogue]
    public TextAsset DefaultDialogueJson;
    public bool EnableDelays = true;

    private ConsoleTyper typer;
    private ConsoleInputManager inputManager;
    private ConsoleButtonManager buttonManager;
    private DialogueFlowController flowController;

    private DialogueNode lastNode;
    private string lastButtonLabel;

    void Start()
    {
        InitializeComponents();
        BindEvents();
        InitializeDialogueFlow();
        flowController.StartFlow();
    }

    void InitializeComponents()
    {
        typer = GetComponent<ConsoleTyper>();
        typer.enableDelays = EnableDelays;

        inputManager = GetComponent<ConsoleInputManager>();
        buttonManager = GetComponent<ConsoleButtonManager>();
    }

    void BindEvents()
    {
        inputManager.OnCommandSubmitted += HandleCommandSubmitted;
        buttonManager.OnButtonSelected += HandleButtonSelected;
        typer.OnStepComplete += NotifyStepFinished;
    }

    void InitializeDialogueFlow()
    {
        DialogueGraph graph = JsonCompressor.Deserialize<DialogueGraph>(DefaultDialogueJson.text);
        flowController = new DialogueFlowController(graph, this, EnableDelays);
        flowController.OnStepOutput += HandleStepOutput;
    }

    void HandleStepOutput(BaseStep step)
    {
        switch (step)
        {
            case TypeStep ts:
                typer.PrintLine(ts);
                break;

            case ButtonStep bs:
                buttonManager.ShowButtons(bs.Buttons);
                typer.ScrollToBottom();
                break;

            case CommandStep cs:
                inputManager.ShowInput();
                typer.ScrollToBottom();
                break;

            default:
                Debug.LogWarning("Unknown step type");
                break;
        }
    }

    void NotifyStepFinished()
    {
        flowController.NotifyStepFinished();
    }

    void HandleCommandSubmitted(string cmd)
    {
        typer.PrintUserLine(cmd);

        bool success = flowController.SubmitCommand(cmd);

        if (!success)
        {
            var prev = flowController.GetPreviousNode();
            if (prev?.Step is TypeStep type)
            {
                typer.PrintLine(type);
            } else if (prev?.Step is ButtonStep && !string.IsNullOrEmpty(lastButtonLabel))
            {
                typer.PrintLine(new TypeStep
                {
                    Lines = new List<TypeLine>
                    {
                        new TypeLine
                        {
                            Text = lastButtonLabel,
                            DisplayType = EDisplayType.UserInput,
                            LogType = ELogType.User,
                            DelayConfig = new StepDelayConfig { CharacterDelay = 0.05f }
                        }
                    }
                });
            }
        }
    }

    void HandleButtonSelected(int index, string label)
    {
        lastButtonLabel = label;
        lastNode = flowController.CurrentNode;

        typer.PrintUserLine(label);
        flowController.SelectButton(index);
    }

    public void LoadGraphByName(string name)
    {
        var asset = Resources.Load<TextAsset>($"Dialogue/{name}");
        if (asset == null)
        {
            Debug.LogError($"Dialogue graph '{name}' niet gevonden in Resources/DialogueGraphs!");
            return;
        }

        var graph = JsonCompressor.Deserialize<DialogueGraph>(asset.text);
        flowController.LoadGraph(graph);
    }
}
