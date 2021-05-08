using deVoid.UIFramework;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
public class PopupMessage : AWindowController<PopupMessageProperties>
{
    [SerializeField] TextMeshProUGUI Header;
    [SerializeField] TextMeshProUGUI Content;
    [SerializeField] TextMeshProUGUI OkText;
    [SerializeField] TextMeshProUGUI CancelText;
    [SerializeField] Button OkButton;
    [SerializeField] Button CancelButton;

    protected override void OnPropertiesSet()
    {
        base.OnPropertiesSet();
        Header.text = Properties.Header;
        Content.text = Properties.Content;
        OkText.text = Properties.OKButtonText;
        CancelText.text = Properties.CancelButtonText;
        if (Properties.OnOK != null) OkButton.onClick.AddListener(() => { Properties.OnOK.Invoke(); });
        if (Properties.OnCancel != null) CancelButton.onClick.AddListener(() => { Properties.OnCancel.Invoke(); });

    }
    protected override void WhileHiding()
    {
        base.WhileHiding();
        OkButton.onClick.RemoveAllListeners();
        CancelButton.onClick.RemoveAllListeners();
    }

    public void Close()
    {
        Frame.CloseCurrentWindow();
    }

}
[System.Serializable]
public class PopupMessageProperties:WindowProperties
{
    public string Header { get; set; }
    public string Content { get; set; }
    public string OKButtonText { get; set; }
    public string CancelButtonText { get; set; }
    public Action OnCancel { get; set; }
    public Action OnOK { get; set; }
    public PopupMessageProperties() { }
    public PopupMessageProperties(string header,string content,string okBtnText="OK",string cancelBtnText="Cancel",Action onOK=null,Action onCancel=null)
    {
        Header = header;
        Content = content;
        OKButtonText = okBtnText;
        CancelButtonText = cancelBtnText;
        OnCancel = onCancel;
        OnOK = onOK;
    }
}