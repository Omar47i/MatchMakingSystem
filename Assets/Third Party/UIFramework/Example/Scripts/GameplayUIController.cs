using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using deVoid.UIFramework;
public class GameplayUIController : AWindowController
{
    public void Exit()
    {
        Frame.CloseCurrentWindow();
    }
    public void Pause()
    {
        ToastMessageProperties toast = new ToastMessageProperties("Can not pause the game!");
        Frame.ShowPanel(ScreenId.Toast, toast);
    }
    public void ShowSettings()
    {
        Frame.OpenWindow(ScreenId.SettingsMenu);
    }
}
