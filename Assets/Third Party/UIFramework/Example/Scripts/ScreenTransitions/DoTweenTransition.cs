using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

namespace deVoid.UIFramework.Examples
{
    public abstract class DoTweenTransition : ATransitionComponent
    {
        public sealed override void Animate(Transform target, Action callWhenFinished)
        {
            AnimateRoutine(target, callWhenFinished);
        }
        public abstract List<Tween> AnimateRoutine(Transform target, Action callWhenFinished);
    }
}