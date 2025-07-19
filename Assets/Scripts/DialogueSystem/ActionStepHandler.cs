using UnityEngine;

public class ActionStepHandler
{
    public ActionStepHandler(DialogueFlowController controller)
    {
        controller.OnActionRequested += HandleAction;
    }

    private void HandleAction(ActionStep step)
    {
        switch (step.ActionType)
        {
            case EActionType.CheckIsPlayerFirstTimePlaying:
                Debug.Log("wow");
                //string key = step.Parameters["key"] as string;
                //int expectedValue = Convert.ToInt32(step.Parameters["value"]);
                //bool hasItem = PlayerPrefs.GetInt(key, 0) == expectedValue;
                //Debug.Log($"PlayerPrefs check: {key} == {expectedValue} ? {hasItem}");
                break;

            //case EActionType.SpawnConsolePrefab:
            //    string prefabName = step.Parameters["prefabName"] as string;
            //    // Your prefab spawning logic here
            //    Debug.Log($"Spawning prefab: {prefabName}");
            //    break;

            //case EActionType.ScrambleConsoleItems:
            //    // Your scrambling logic here
            //    Debug.Log("Scrambling console items...");
            //    break;

            //// Add more cases as needed
            //default:
            //    Debug.LogWarning($"Unknown action type: {step.ActionType}");
            //    break;
        }
    }
}