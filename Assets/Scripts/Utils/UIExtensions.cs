using System.Text.RegularExpressions;
using UnityEngine.UIElements;

public static class UIExtensions
{
    public static void AddClass(this VisualElement self, string className)
    {
        self.AddToClassList(className);
    }

    public static void RemoveClass(this VisualElement self, string className)
    {
        self.RemoveFromClassList(className);
    }

    public static string StripRichText(string input)
    {
        return Regex.Replace(input, "<.*?>", string.Empty);
    }

    public static string GetDialogueStepTerminalPrefix(TypeStep step)
    {
        var prefix = step.LogType switch
        {
            ELogType.System => "[SYSTEM]",
            ELogType.Input => "[INPUT]",
            _ => ""
        };

        return string.IsNullOrEmpty(prefix) ? prefix : prefix.PadRight(8);
    }

    public static string GetDialogueColorTag(ELogType logType)
    {
        return logType switch
        {
            ELogType.System => "<color=#ffffff>",
            ELogType.Input => "<color=#00ffff>",
            ELogType.User => "<color=#00ffff>",
            _ => "<color=#ffffff>"
        };
    }

    public static void SetDisplay(this VisualElement ve, bool visible)
    {
        ve.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
