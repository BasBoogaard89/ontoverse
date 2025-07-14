using UnityEngine;

public class MainPanelDialogTest
{
    public bool ClearPlayerPrefs;

    void Start()
    {
    }

    void Boot()
    {
        //bool disableIntro = PlayerPrefs.GetInt("DisableIntro") == 1;

        //Typer.EnqueueSequence(new TerminalSequence(new List<DialogueStep>
        //{
        //    new(EDialogueStepType.Prompt, "./ontoverse.exe", 1f),
        //    new(EDialogueStepType.Wait, "", 1f),
        //    new(EDialogueStepType.Type, "Booting kernel modules...", 0f, 0f, logType: ELogType.System),
        //    new(EDialogueStepType.Type, "Loading assets...", .1f, 0f, logType: ELogType.System),
        //    new(EDialogueStepType.Type, "Loading data...", .1f, 0f, logType: ELogType.System),
        //    new(EDialogueStepType.Wait, "", 2f),
        //    new(EDialogueStepType.Type, GetAsciiArt(), 0f, 0f),
        //    new(EDialogueStepType.FakeUserInput, "login --user exeistance --pw B1gBitti3sL0ver!", 1f), 
        //    new(EDialogueStepType.Type, "Access granted", 1f, 0f, logType: ELogType.System),
        //    new(EDialogueStepType.Type, "Loading user profile", 3f, 0f, logType: ELogType.System),
        //    new(EDialogueStepType.Wait, "", 3f),
        //}, HasDisabledIntro ? StartGame : StartIntro));
    }

    bool HasDisabledIntro => PlayerPrefs.GetInt("DisableIntro") == 1;

    void StartIntro()
    {
        //Typer.EnqueueSequence(new TerminalSequence(new List<DialogueStep>
        //{
        //    new(EDialogueStepType.Type, "Impossible! Perhaps the archives are incomplete...", 1f, 0f, logType: ELogType.System),
        //    new(EDialogueStepType.Type, "Loading user profile even harder", 1f, 0f, logType: ELogType.System),
        //    new(EDialogueStepType.Type, "Sudo load user profile?", 3f, 0f, logType: ELogType.System),
        //    new(EDialogueStepType.Type, "Strange... OK well, let's pretend that never happened. Hello fellow existing user somehow logging in without a profile to be found!", 3f, 0f, logType: ELogType.System),
        //    new(EDialogueStepType.Type, $"Press {WrapInTag("[Space]")} to toggle the User Console. It might be your enemy at first but you'll learn to use and appreciate it soon enough!", 2f, 0f, logType: ELogType.Input),
        //}));


        PlayerPrefs.SetInt("DisableIntro", 1);
        PlayerPrefs.Save();
    }

    void StartGame()
    {
        //Typer.EnqueueSequence(new TerminalSequence(new List<DialogueStep>
        //{
        //    new(EDialogueStepType.Type, $"According to my knowledge, it is not your first time around this system. Press {WrapInTag("[Space]")} to toggle the User Console and take it from there. Remember that help is just one {WrapInTag("`help`")} command away!", 2f, 0f, logType: ELogType.Input),
        //}));
    }

    void HasEnabledUserConsole()
    {  
        //Typer.EnqueueSequence(new TerminalSequence(new List<DialogueStep>
        //{
        //    new(EDialogueStepType.Type, $"Awesome! Type `help` and press {WrapInTag("[Enter]")} or click {WrapInTag("[Run command]")}.", 0f, 0f, logType: ELogType.Input),
        //}));
    }

    private string WrapInTag(string text) 
    {
        return $"<color=#ffff00>{text}</color>";
    }

    private string GetAsciiArt()
    {
        return @"
=======================================
<size=8px>
 ██████  ███    ██ ████████  ██████  ██    ██ ███████ ██████  ███████ ███████ 
██    ██ ████   ██    ██    ██    ██ ██    ██ ██      ██   ██ ██      ██      
██    ██ ██ ██  ██    ██    ██    ██ ██    ██ █████   ██████  ███████ █████   
██    ██ ██  ██ ██    ██    ██    ██  ██  ██  ██      ██   ██      ██ ██      
 ██████  ██   ████    ██     ██████    ████   ███████ ██   ██ ███████ ███████ 
</size>
=======================================

";
    }
}
