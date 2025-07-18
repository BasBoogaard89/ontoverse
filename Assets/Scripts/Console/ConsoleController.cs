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

    //void Start()
    //{
    //    typer = GetComponent<ConsoleTyper>();
    //    inputManager = GetComponent<ConsoleInputManager>();
    //    buttonManager = GetComponent<ConsoleButtonManager>();

    //    inputManager.OnCommandSubmitted += HandleCommand;
    //    buttonManager.OnButtonSelected += HandleButton;

    //    DialogueGraph graph = JsonUtility.FromJson<DialogueGraph>(defaultDialogueJson.text);
    //    flow = new DialogueFlowController(graph);

    //    flow.OnTextOutput += typer.PrintLine;
    //    flow.OnButtonsPresented += (buttons) =>
    //    {
    //        buttonManager.ShowButtons(buttons);
    //        typer.ScrollToBottom();
    //    };
    //    flow.OnCommandRequired += () =>
    //    {
    //        inputManager.ShowInput();
    //        typer.ScrollToBottom();
    //    };

    //    typer.OnStepComplete += flow.NotifyStepFinished;

    //    flow.StartFlow();
    //}

    //void HandleCommand(string cmd)
    //{
    //    typer.PrintUserLine(cmd);

    //    bool valid = validator.Validate(cmd, flow.currentNode.Step);

    //    flow.ResumeFlow(valid); 
    //}

    //void HandleButton(int index)
    //{
    //    string label = flow.currentNode.Step.Buttons[index].Label;
    //    typer.PrintUserLine(label);
    //    flow.SelectButton(index);
    //}
}