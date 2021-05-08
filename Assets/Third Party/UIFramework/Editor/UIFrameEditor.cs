using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using deVoid.UIFramework;
[CustomEditor(typeof(UIFrame))]
public class UIFrameEditor : Editor
{
    VisualElement rootElement;
    VisualTreeAsset modulesVisualTree;
    public void OnEnable()
    {
        rootElement = new VisualElement();
        modulesVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Third Party/UIFramework/Editor/UIFrame.uxml");
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Third Party/UIFramework/Editor/UIFrame.uss");
        rootElement.styleSheets.Add(styleSheet);
    }
    public override VisualElement CreateInspectorGUI()
    {
        rootElement.Clear();
        modulesVisualTree.CloneTree(rootElement);
        rootElement.Bind(serializedObject);
        Label currentScreenlbl = rootElement.Q<Label>("currentScreenlbl");
        //Label currentScreenlbl = rootElement.Query<Label>(name: "currentScreenlbl").First();
        var schedularAction = rootElement.schedule.Execute(() =>
        {
            if ((target as UIFrame).CurrentWindow == (ScreenId)(-1))
                currentScreenlbl.text = "None";
            else
                currentScreenlbl.text = (target as UIFrame).CurrentWindow.ToString();

            UpdateStackPreview();
            UpdateQueuePreview();
        });
        schedularAction.Every(100);

        //var stackContainer=rootElement.Q("StackContainer");
        //Label stackElemnt = new Label("Dynamic Element");
        //stackElemnt.AddToClassList("stack-item");
        //stackContainer.Add(stackElemnt);

        return rootElement;
    }
    List<ScreenId> stackItems;
    private void UpdateStackPreview()
    {
        if (stackItems == null) stackItems = new List<ScreenId>();
        stackItems.Clear();
        UIFrame frame = target as UIFrame;
        foreach (var window in frame.WindowHistory)
        {
            stackItems.Add(window.Screen.ScreenId);
        }
        if (stackItems.Count == 0) return;
        var stackContainer = rootElement.Q("StackContainer");
        if (stackContainer.childCount < stackItems.Count)
        {
            for (int i = stackContainer.childCount; i < stackItems.Count; i++)
            {
                Label stackElemnt = new Label(stackItems[i].ToString());
                stackElemnt.AddToClassList("stack-item");
                stackContainer.Add(stackElemnt);
            }
        }
        else if (stackContainer.childCount > stackItems.Count)
        {

            for (int i = stackContainer.childCount - 1; i >= stackItems.Count; i--)
            {
                stackContainer.RemoveAt(i);
            }
        }
        int index = 0;

        foreach (var child in stackContainer.Children())
        {
            (child as Label).text = stackItems[index++].ToString();
        }
    }

    List<ScreenId> queueItems;
    private void UpdateQueuePreview()
    {
        if (queueItems == null) queueItems = new List<ScreenId>();
        queueItems.Clear();
        UIFrame frame = target as UIFrame;
        foreach (var window in frame.WindowQueue)
        {
            queueItems.Add(window.Screen.ScreenId);
        }
        if (queueItems.Count == 0) return;
        var queueContainer = rootElement.Q("QueueContainer");
        if (queueContainer.childCount < queueItems.Count)
        {
            for (int i = queueContainer.childCount; i < queueItems.Count; i++)
            {
                Label stackElemnt = new Label(queueItems[i].ToString());
                stackElemnt.AddToClassList("stack-item");
                queueContainer.Add(stackElemnt);
            }
        }
        else if (queueContainer.childCount > queueItems.Count)
        {
            for (int i = queueItems.Count; i < queueContainer.childCount; i++)
            {
                queueContainer.RemoveAt(i);
            }
        }
        int index = 0;
        foreach (var child in queueContainer.Children())
        { (child as Label).text = queueItems[index++].ToString(); }
    }
}
