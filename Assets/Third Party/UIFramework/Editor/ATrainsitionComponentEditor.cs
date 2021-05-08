using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using deVoid.UIFramework;
using deVoid.UIFramework.Examples;
using DG.DOTweenEditor;
using DG.Tweening;

[CustomEditor(typeof(DoTweenTransition),true), CanEditMultipleObjects]
public class ATrainsitionComponentEditor :Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Show Transition animation"))
        {
            DoTweenTransition transitionComp = (target as DoTweenTransition);
            List<Tween> tweens = transitionComp.AnimateRoutine(transitionComp.transform, null);
            if (tweens == null)
            {
                Debug.LogWarning("Can't preview the animation, The transition AnimateRoutine function should assign the tween property in order to preview it in edit mode ");
                return;
            }
            foreach (var tween in tweens)
            {
                DOTweenEditorPreview.PrepareTweenForPreview(tween);
                DOTweenEditorPreview.Start();
            }
        }
    }
}
