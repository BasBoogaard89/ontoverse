using UnityEngine;

[RequireComponent(typeof(ConsoleTyper))]
[RequireComponent(typeof(ConsoleInputManager))]
[RequireComponent(typeof(ConsoleButtonManager))]
public class ConsoleController : MonoBehaviour
{
    [JsonDialogue]
    [SerializeField] TextAsset defaultDialogueJson;
    ConsoleTyper typer;
    ConsoleInputManager inputManager;
    ConsoleButtonManager buttonManager;

    DialogueFlowController flow;
    CommandValidator validator = new();

    void Start()
    {
        typer = GetComponent<ConsoleTyper>();
        inputManager = GetComponent<ConsoleInputManager>();
        buttonManager = GetComponent<ConsoleButtonManager>();

        inputManager.OnCommandSubmitted += HandleCommand;
        buttonManager.OnButtonSelected += HandleButton;

        DialogueGraph graph = JsonCompressor.Deserialize<DialogueGraph>(defaultDialogueJson.text);
        flow = new DialogueFlowController(graph);

        flow.OnStepOutput += HandleStep;
        flow.OnButtonsPresented += (buttons) =>
        {
            buttonManager.ShowButtons(buttons);
            typer.ScrollToBottom();
        };
        flow.OnCommandRequired += () =>
        {
            inputManager.ShowInput();
            typer.ScrollToBottom();
        };

        typer.OnStepComplete += flow.NotifyStepFinished;

        flow.StartFlow();
    }

    void HandleCommand(string cmd)
    {
        typer.PrintUserLine(cmd);

        bool valid = validator.Validate(cmd, (CommandStep)flow.CurrentNode.Step);

        flow.ResumeFlow(valid);
    }

    void HandleButton(int index)
    {
        string label = ((ButtonStep)flow.CurrentNode.Step).Buttons[index].Label;
        typer.PrintUserLine(label);
        flow.SelectButton(index);
    }

    void HandleStep(BaseStep step)
    {
        if (step is TypeStep ts)
            typer.PrintLine(ts);
    }
}