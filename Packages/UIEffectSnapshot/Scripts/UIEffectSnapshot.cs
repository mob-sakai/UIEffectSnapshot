using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SamplingRate = Coffee.UIExtensions.UIEffectSnapshotRequest.SamplingRate;
using EffectMode = Coffee.UIExtensions.UIEffectSnapshotRequest.EffectMode;
using ColorMode = Coffee.UIExtensions.UIEffectSnapshotRequest.ColorMode;
using BlurMode = Coffee.UIExtensions.UIEffectSnapshotRequest.BlurMode;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Coffee.UIEffectSnapshot.Editor")]

namespace Coffee.UIExtensions
{
    /// <summary>
    /// UIEffectSnapshot
    /// </summary>
    [AddComponentMenu("UI/UIEffectSnapshot", 200)]
    public class UIEffectSnapshot : RawImage
    {
        [SerializeField] UIEffectSnapshotRequest m_Request = new UIEffectSnapshotRequest();


        [SerializeField] private bool m_Maskable = true;

        [Tooltip("Fits graphic size to screen on captured.")] [SerializeField]
        bool m_FitToScreen = true;

        [Tooltip("Capture automatically on enable.")] [SerializeField]
        bool m_CaptureOnEnable = false;

        [Tooltip("Capture and show global captured texture.\nIf enabled, show the texture captured from scripts.")] [SerializeField]
        bool m_GlobalMode = false;

        /// <summary>
        /// Snapshot request.
        /// </summary>
        public UIEffectSnapshotRequest request
        {
            get { return m_Request; }
        }

        /// <summary>
        /// Captured texture.
        /// </summary>
        public RenderTexture capturedTexture
        {
            get { return m_GlobalMode ? UIEffectSnapshotProcesser.instance.globalCapturedTexture : request.renderTexture; }
        }

        /// <summary>
        /// Global captured texture.
        /// </summary>
        public RenderTexture globalCapturedTexture
        {
            get { return UIEffectSnapshotProcesser.instance.globalCapturedTexture; }
        }

        /// <summary>
        /// Fits graphic size to screen on captured.
        /// </summary>
        public bool fitToScreen
        {
            get { return m_FitToScreen; }
            set { m_FitToScreen = value; }
        }

        /// <summary>
        /// Capture automatically on enable.
        /// </summary>
        public bool captureOnEnable
        {
            get { return m_CaptureOnEnable; }
            set { m_CaptureOnEnable = value; }
        }

        /// <summary>
        /// Global mode.
        /// </summary>
        public bool globalMode
        {
            get { return m_GlobalMode; }
            set { m_GlobalMode = value; }
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        protected override void OnEnable()
        {
            maskable = m_Maskable;
            base.OnEnable();

            // Capture on enable.
            if (m_CaptureOnEnable && Application.isPlaying)
            {
                Capture();
            }

            // Global mode.
            if (m_GlobalMode && Application.isPlaying)
            {
                texture = UIEffectSnapshotProcesser.instance.globalCapturedTexture;
            }

            FitToScreen(); // Fit to screen.
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (m_CaptureOnEnable && Application.isPlaying && request.renderTexture)
            {
                RenderTexture.ReleaseTemporary(request.renderTexture);
                request.renderTexture = null;
                texture = null;
            }
        }

        /// <summary>
        /// This function is called when the MonoBehaviour will be destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (request.renderTexture && !request.globalMode)
            {
                RenderTexture.ReleaseTemporary(request.renderTexture);
            }
        }

        protected override void UpdateMaterial()
        {
            m_Maskable = maskable;
            base.UpdateMaterial();
        }

        /// <summary>
        /// Callback function when a UI element needs to generate vertices.
        /// </summary>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            // When not displaying, clear vertex.
            if (texture == null || color.a < 1 / 255f || canvasRenderer.GetInheritedAlpha() < 1 / 255f)
            {
                vh.Clear();
            }
            else
            {
                base.OnPopulateMesh(vh);
                var count = vh.currentVertCount;
                var vt = default(UIVertex);
                var c = color;
                for (var i = 0; i < count; i++)
                {
                    vh.PopulateUIVertex(ref vt, i);
                    vt.color = c;
                    vh.SetUIVertex(vt, i);
                }
            }
        }

        private void FitToScreen()
        {
            if (!m_FitToScreen || !canvas) return;
            var rootCanvas = canvas.rootCanvas;
            var rootTransform = rootCanvas.transform as RectTransform;
            var size = rootTransform.rect.size;
            var rectTransform = transform as RectTransform;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            rectTransform.position = rootTransform.position;
        }

        public static void CaptureForGlobal(UIEffectSnapshotRequest request, Action<UIEffectSnapshotRequest> callback = null)
        {
            if (request == null) return;
            request.globalMode = true;
            if (callback != null)
                request.postAction += callback;
            UIEffectSnapshotProcesser.instance.Register(request);
        }

        public static void CaptureForGlobal(
            EffectMode effectMode = EffectMode.None, float effectFactor = 1f,
            ColorMode colorMode = ColorMode.Multiply, float colorFactor = 1f, Color effectColor = default(Color),
            BlurMode blurMode = BlurMode.FastBlur, float blurFactor = 1f, int blurIterations = 2,
            SamplingRate samplingRate = SamplingRate.x2, SamplingRate reductionRate = SamplingRate.x2, FilterMode filterMode = FilterMode.Bilinear,
            Action<UIEffectSnapshotRequest> callback = null
        )
        {
            if (effectColor == default(Color))
                effectColor = Color.white;

            var request = new UIEffectSnapshotRequest()
            {
                effectMode = effectMode,
                effectFactor = effectFactor,
                colorMode = colorMode,
                colorFactor = colorFactor,
                effectColor = effectColor,
                blurMode = blurMode,
                blurFactor = blurFactor,
                blurIterations = blurIterations,
                samplingRate = samplingRate,
                reductionRate = reductionRate,
                filterMode = filterMode,
            };
            CaptureForGlobal(request, callback);
        }

        /// <summary>
        /// Capture rendering result.
        /// </summary>
        public void Capture()
        {
            request.globalMode = globalMode;
            request.postAction = r =>
            {
                texture = r.renderTexture;
                FitToScreen(); // Fit to screen.

#if UNITY_EDITOR
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (Application.isPlaying || !this) return;
                    UnityEditor.EditorUtility.SetDirty(this);
                };
#endif
            };
            UIEffectSnapshotProcesser.instance.Register(request);
        }


        /// <summary>
        /// Capture rendering result.
        /// </summary>
        public void Capture(Action<UIEffectSnapshotRequest> callback)
        {
            Capture();
            if (callback != null)
                request.postAction += callback;
        }

        /// <summary>
        /// Release captured image.
        /// </summary>
        public void Release()
        {
            var rt = request.renderTexture;
            if (!rt) return;

            // rt.Release();
            RenderTexture.ReleaseTemporary(rt);
            request.renderTexture = null;
            texture = null;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            request.blurIterations = 3;
            request.filterMode = FilterMode.Bilinear;
            request.samplingRate = UIEffectSnapshotRequest.SamplingRate.x1;
            request.reductionRate = UIEffectSnapshotRequest.SamplingRate.x1;
            base.Reset();
        }

        protected override void OnValidate()
        {
            maskable = m_Maskable;
            base.OnValidate();
        }
#endif
    }
}
