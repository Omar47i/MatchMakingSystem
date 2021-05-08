using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using deVoid.UIFramework;
public class Settings : AWindowController
{
    bool shouldClose = false;
    public void Close()
    {
        Frame.CloseCurrentWindow();
    }
    public void ConfirmSaving()
    {
        PopupMessageProperties popupProp = new PopupMessageProperties("Confirmation", "Are you sure you want to save the new settings", "Yes", "No");
        popupProp.OnOK = () => {
            shouldClose = true;
        };
        popupProp.OnCancel = () => {
            shouldClose = false;
        };
        Frame.OpenWindow(ScreenId.PopupMesage, popupProp);
    }
    public void ConfirmCancellation()
    {
        PopupMessageProperties popupProp = new PopupMessageProperties("Confirmation", "Are you sure you want to discar the new settings", "Yes", "No");
        popupProp.OnOK = () => {
            shouldClose = true;

        };
        popupProp.OnCancel = () => {
            shouldClose = false;

        };
        Frame.OpenWindow(ScreenId.PopupMesage, popupProp);

    }
    protected override void OnPropertiesSet()
    {
        base.OnPropertiesSet();
        if(shouldClose)
        {
            shouldClose = false;
            //we should wait until the end of the frame so this window become the current window
            StartCoroutine(CloseCoroutine());
        }
    }
    IEnumerator CloseCoroutine()
    {
        yield return new WaitForEndOfFrame();
        Frame.CloseWindow(ScreenId.SettingsMenu);
    }
}
