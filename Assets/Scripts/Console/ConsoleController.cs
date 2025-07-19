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

    ConsoleTyper typer;
    ConsoleInputManager inputManager;
    ConsoleButtonManager buttonManager;
    DialogueFlowController flowController;

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
        flowController.OnButtonsPresented += HandleButtonsPresented;
        flowController.OnCommandRequired += HandleCommandRequired;

        _ = new ActionStepHandler(flowController);
    }

    void HandleStepOutput(BaseStep step)
    {
        if (step is TypeStep ts)
            typer.PrintLine(ts);
    }

    void HandleButtonsPresented(List<ButtonData> buttons)
    {
        buttonManager.ShowButtons(buttons);
        typer.ScrollToBottom();
    }

    void HandleCommandRequired()
    {
        inputManager.ShowInput();
        typer.ScrollToBottom();
    }

    void NotifyStepFinished()
    {
        flowController.NotifyStepFinished();
    }

    void HandleCommandSubmitted(string cmd)
    {
        typer.PrintUserLine(cmd);

        flowController.SubmitCommand(cmd);
    }

    void HandleButtonSelected(int index, string label)
    {
        typer.PrintUserLine(label);
        flowController.SelectButton(index);
    }
}