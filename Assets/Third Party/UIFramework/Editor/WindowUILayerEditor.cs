using deVoid.UIFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WindowUILayer), true), CanEditMultipleObjects]
public class WindowUILayerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.HelpBox(new GUIContent("Current screen"), wide: true);
        GUILayout.BeginVertical("HelpBox");
        GUILayout.Label("CurrentScreen");
        GUILayout.EndVertical();
        GUIStyle style = new GUIStyle(GUI.skin.box); //Create a copy of the default box style
        style.padding = new RectOffset(1, 1, 1, 1); //Reset the padding to zero
        style.margin = new RectOffset();  //Reset the margin to zero
        style.normal.textColor = Color.red;
        GUILayout.Label("CurrentScreen", style);

        GUILayout.Label("CurrentScreen", EditorStyles.helpBox, GUILayout.Width(100));


        WindowUILayer windowLAyer = target as WindowUILayer;
        if (windowLAyer.CurrentWindow != null) GUILayout.Label(windowLAyer.CurrentWindow.ScreenId.ToString(), EditorStyles.helpBox, GUILayout.Width(100));

        


    }
}
