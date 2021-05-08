using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using deVoid.UIFramework;

public class MainMenuController : AWindowController
{
    public void OpenSettingsWindow()
    {
         Frame.OpenWindow(ScreenId.SettingsMenu);
    }
    public void ShowToastMessage()
    {
        Frame.ShowPanel(ScreenId.Toast, new ToastMessageProperties("This is a toast message!"));
    }
    public void StartGame()
    {
        Frame.OpenWindow(ScreenId.GameplayUI);
    }
}
