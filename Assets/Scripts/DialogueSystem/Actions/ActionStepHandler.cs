//using System;
//using System.Collections.Generic;
//using UnityEngine;

//namespace Ontoverse.DialogueSystem
//{
    //public class ActionStepHandler
    //{
    //    private ConsoleController consoleController;
    //    private DialogueFlowController flowController;
    //    private Dictionary<EActionType, Func<ActionStep, bool?>> actions = new();

    //    public ActionStepHandler(ConsoleController consoleController, DialogueFlowController flowController)
    //    {
    //        this.consoleController = consoleController;
    //        this.flowController = flowController;

    //        actions[EActionType.CheckHideIntro] = HandleCheckHideIntro;
    //        actions[EActionType.LoadGraph] = step => { HandleLoadGraph(step); return null; };

    //        flowController.OnActionRequested += HandleAction;
    //    }

    //    private void HandleAction(ActionStep step)
    //    {
    //        if (actions.TryGetValue(step.ActionType, out var handler))
    //        {
    //            bool? result = handler(step);
    //            if (result.HasValue)
    //                flowController.HandleAction(step, result.Value);
    //        } else
    //        {
    //            Debug.LogWarning($"No handler for action {step.ActionType}");
    //        }
    //    }

    //    private bool? HandleCheckHideIntro(ActionStep step)
    //    {
    //        if (PlayerPrefs.GetInt("HideInto") == 0)
    //        {
    //            PlayerPrefs.SetInt("HideInto", 1);
    //            PlayerPrefs.Save();
    //            return true;
    //        } else
    //        {
    //            return false;
    //        }
    //    }

    //    private void HandleLoadGraph(ActionStep step)
    //    {
    //        string name = (string)step.Parameters["name"];
    //        consoleController.LoadGraphByName(name);
    //    }
    //}
//}