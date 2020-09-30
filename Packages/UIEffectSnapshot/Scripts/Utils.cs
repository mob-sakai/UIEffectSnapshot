using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Coffee.UIExtensions
{
    internal static class UIEffectSnapshotUtils
    {
#if UNITY_EDITOR
        public static void DelayCall(Action callback)
        {
            var callTime = EditorApplication.timeSinceStartup + 0.1f;
            EditorApplication.CallbackFunction action = null;
            EditorApplication.update += action = () =>
            {
                if (EditorApplication.timeSinceStartup < callTime) return;

                EditorApplication.update -= action;
                callback();
            };
        }
#endif

        public static Coroutine StartCoroutineSafety(IEnumerator coroutine)
        {
            return coroutine == null
                ? null
                : UIEffectSnapshotUpdater.instance.StartCoroutine(coroutine);
        }

        public static void StopCoroutineSafety(Coroutine coroutine)
        {
            if (coroutine == null) return;
            UIEffectSnapshotUpdater.instance.StopCoroutine(coroutine);
        }
    }
}
