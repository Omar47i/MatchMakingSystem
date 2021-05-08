using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using deVoid.UIFramework;
using System;

public class ToastMessageController : APanelController<ToastMessageProperties>
{
    [SerializeField] TextMeshProUGUI messageText;
    protected override void OnPropertiesSet()
    {
        base.OnPropertiesSet();
        messageText.text = Properties.Message;
        InTransitionFinished += HideCoroutine;
        
    }

    private void HideCoroutine(IUIScreenController obj)
    {
        StartCoroutine(HideMe());
    }

    IEnumerator HideMe()
    {
        yield return new WaitForSeconds(2);
        Frame.HidePanel(ScreenId.Toast);
    }
}
[System.Serializable]
public class ToastMessageProperties:PanelProperties
{
    public string Message { get; set; }
    public ToastMessageProperties(string message)
    {
        Message = message;
        Priority = PanelPriority.Prioritary;
    }
}