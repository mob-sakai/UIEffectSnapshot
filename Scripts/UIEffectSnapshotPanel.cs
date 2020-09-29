using System;
using System.Collections;
using UnityEngine;

namespace Coffee.UIExtensions
{
    [ExecuteAlways]
    [RequireComponent(typeof(CanvasGroup))]
    public class UIEffectSnapshotPanel : MonoBehaviour
    {
        [SerializeField] private UIEffectSnapshot[] m_Snapshots = new UIEffectSnapshot[0];
        [Range(0.01f, 5)] [SerializeField] private float m_TransitionDuration = 0.25f;
        [SerializeField] private bool m_ShowOnEnable = true;
        [SerializeField] private bool m_DeactivateOnHidden = false;

        private CanvasGroup _canvasGroup;
        private Coroutine _coroutine;

        private CanvasGroup canvasGroup
        {
            get { return _canvasGroup ? _canvasGroup : _canvasGroup = GetComponent<CanvasGroup>(); }
        }

        public UIEffectSnapshot[] snapshots
        {
            get { return m_Snapshots; }
            set { m_Snapshots = value; }
        }

        public bool showOnEnable
        {
            get { return m_ShowOnEnable; }
            set { m_ShowOnEnable = value; }
        }

        public bool deactivateOnHidden
        {
            get { return m_DeactivateOnHidden; }
            set { m_DeactivateOnHidden = value; }
        }

        private void OnEnable()
        {
            if (m_ShowOnEnable)
                Show();
        }

        private void OnDestroy()
        {
            UIEffectSnapshotUtils.StopCoroutineSafety(_coroutine);
        }

        public void Show()
        {
            Show(null);
        }

        public void Show(Action callback)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                canvasGroup.alpha = 0;
                UIEffectSnapshotUtils.DelayCall(() =>
                {
                    if (!this) return;
                    Capture();
                    canvasGroup.alpha = 1;
                });
                return;
            }
#endif

            UIEffectSnapshotUtils.StopCoroutineSafety(_coroutine);
            UIEffectSnapshotUtils.StartCoroutineSafety(Co_Show(m_TransitionDuration, callback));
        }

        public void Hide()
        {
            Hide(null);
        }

        public void Hide(Action callback = null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Release();
                return;
            }
#endif

            UIEffectSnapshotUtils.StopCoroutineSafety(_coroutine);
            UIEffectSnapshotUtils.StartCoroutineSafety(Co_Hide(m_TransitionDuration, callback));
        }

        private IEnumerator Co_Show(float duration, Action callback)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 0;
            Capture();
            yield return null;

            var speed = 1 / Mathf.Max(0.001f, duration);
            while (canvasGroup.alpha < 1)
            {
                canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha + Time.deltaTime * speed);
                yield return null;
            }

            _coroutine = null;
            if (callback != null)
                callback();
        }

        private IEnumerator Co_Hide(float duration, Action callback)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 1;
            var speed = 1 / Mathf.Max(0.001f, duration);
            yield return null;

            while (0 < canvasGroup.alpha)
            {
                canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha - Time.deltaTime * speed);
                yield return null;
            }

            _coroutine = null;
            Release();
            if (m_DeactivateOnHidden)
                gameObject.SetActive(false);

            if (callback != null)
                callback();
        }

        private void Capture()
        {
            foreach (var snapshot in m_Snapshots)
            {
                if (snapshot)
                    snapshot.Capture();
            }
        }

        private void Release()
        {
            foreach (var snapshot in m_Snapshots)
            {
                if (snapshot)
                    snapshot.Release();
            }
        }

        public void Refresh()
        {
            m_Snapshots = GetComponentsInChildren<UIEffectSnapshot>();
        }
    }
}
