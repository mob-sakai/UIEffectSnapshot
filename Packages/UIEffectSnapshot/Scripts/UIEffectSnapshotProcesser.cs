using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Coffee.UIExtensions
{
    [ExecuteAlways]
    [AddComponentMenu("")]
    internal class UIEffectSnapshotProcesser : MonoBehaviour
    {
        static UIEffectSnapshotProcesser s_Instance;
        private readonly List<UIEffectSnapshotRequest> s_Requests = new List<UIEffectSnapshotRequest>();
        private static int s_CopyId;
        private static int s_EffectId1;
        private static int s_EffectId2;
        private static int s_EffectFactorId;
        private static int s_ColorFactorId;
        private static RenderTexture s_GlobalRt;
        private static Shader s_EffectShader;


#if UNITY_2018_3_OR_NEWER && UNITY_EDITOR
        static UIEffectSnapshotProcesser s_InstanceForPrefab;

        private static UIEffectSnapshotProcesser InstanceForPrefab
        {
            get
            {
                // If current scene is prefab mode, create OverlayCamera for editor.
                var prefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage == null || !prefabStage.scene.isLoaded) return null;
                if (s_InstanceForPrefab) return s_InstanceForPrefab;

                s_InstanceForPrefab = Create();
                s_InstanceForPrefab.name += " (For Prefab Stage)";
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(s_InstanceForPrefab.gameObject, prefabStage.scene);

                return s_InstanceForPrefab;
            }
        }
#endif

        public static UIEffectSnapshotProcesser instance
        {
            get
            {
#if UNITY_2018_3_OR_NEWER && UNITY_EDITOR
                var inst = InstanceForPrefab;
                if (inst) return inst;
#endif
                // Find instance in scene, or create new one.
                return s_Instance
                    ? s_Instance
                    : (s_Instance = FindObjectOfType<UIEffectSnapshotProcesser>() ?? Create());
            }
        }

        public RenderTexture globalCapturedTexture
        {
            get { return s_GlobalRt; }
        }

        private static UIEffectSnapshotProcesser Create()
        {
            var gameObject = new GameObject()
            {
                name = typeof(UIEffectSnapshotProcesser).Name,
                hideFlags = HideFlags.HideAndDontSave,
            };

#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            {
                DontDestroyOnLoad(gameObject);
            }
            var inst = gameObject.AddComponent<UIEffectSnapshotProcesser>();
            return inst;
        }

        public void Register(UIEffectSnapshotRequest request)
        {
            s_Requests.Add(request);
        }

        private void OnEnable()
        {
            // Cache some ids.
            if (s_CopyId == 0)
            {
                s_CopyId = Shader.PropertyToID("_UIEffectSnapshot_ScreenCopyId");
                s_EffectId1 = Shader.PropertyToID("_UIEffectSnapshot_EffectId1");
                s_EffectId2 = Shader.PropertyToID("_UIEffectSnapshot_EffectId2");

                s_EffectFactorId = Shader.PropertyToID("_EffectFactor");
                s_ColorFactorId = Shader.PropertyToID("_ColorFactor");
                s_EffectShader = Shader.Find("Hidden/UIEffectSnapshot");
            }

            Canvas.willRenderCanvases -= Capture;
            Canvas.willRenderCanvases += Capture;
        }

        private void Capture()
        {
            Profiler.BeginSample("[UIEffectSnapshot] Process");
            for (var i = 0; i < s_Requests.Count; i++)
            {
                try
                {
                    Capture(s_Requests[i]);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            Profiler.EndSample();
            s_Requests.Clear();
        }


        private static Material GetMaterial(UIEffectSnapshotRequest request)
        {
            var material = new Material(s_EffectShader);
            material.shaderKeywords = new object[] {request.effectMode, request.colorMode, request.blurMode}
                .Where(x => 0 < (int) x)
                .Select(x => x.ToString().ToUpper())
                .ToArray();
            return material;
        }

        /// <summary>
        /// Gets the size of the Sampling.
        /// </summary>
        public static void GetSamplingSize(UIEffectSnapshotRequest.SamplingRate rate, out int w, out int h)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var res = UnityEditor.UnityStats.screenRes.Split('x');
                w = Mathf.Max(64, int.Parse(res[0]));
                h = Mathf.Max(64, int.Parse(res[1]));
            }
            else
#endif
            {
                w = Screen.width;
                h = Screen.height;
            }

            if (rate == UIEffectSnapshotRequest.SamplingRate.None)
                return;

            var aspect = (float) w / h;
            if (w < h)
            {
                h = Mathf.ClosestPowerOfTwo(h / (int) rate);
                w = Mathf.CeilToInt(h * aspect);
            }
            else
            {
                w = Mathf.ClosestPowerOfTwo(w / (int) rate);
                h = Mathf.CeilToInt(w / aspect);
            }
        }

        /// <summary>
        /// Capture rendering result.
        /// </summary>
        private static void Capture(UIEffectSnapshotRequest request)
        {
            // If size of result RT has changed, release it.
            int w, h;
            GetSamplingSize(request.samplingRate, out w, out h);
            var rt = request.globalMode ? s_GlobalRt : request.renderTexture;
            if (rt && (rt.width != w || rt.height != h))
            {
                RenderTexture.ReleaseTemporary(rt);
                rt = null;
            }

            // Generate RT for result.
            if (rt == null)
            {
                rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
                rt.filterMode = request.filterMode;
                rt.useMipMap = false;
                rt.wrapMode = TextureWrapMode.Clamp;

                if (request.globalMode)
                {
                    s_GlobalRt = rt;
                }

                request.renderTexture = rt;
            }

            // Setup command buffer.
            SetupCommandBuffer(request);
        }

        private static void SetupCommandBuffer(UIEffectSnapshotRequest request)
        {
            // [1] Capture from back buffer (back buffer -> copied screen).
            int w, h;
            GetSamplingSize(UIEffectSnapshotRequest.SamplingRate.None, out w, out h);

            var cb = request.commandBuffer = new CommandBuffer();
            cb.GetTemporaryRT(s_CopyId, w, h, 0, request.filterMode);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var gameViewRt = Resources.FindObjectsOfTypeAll<RenderTexture>().FirstOrDefault(x => x.name == "GameView RT");
                if (!gameViewRt)
                {
                    cb.Release();
                    return;
                }

                cb.Blit(gameViewRt, s_CopyId);
            }
            else
#endif
            {
                cb.Blit(BuiltinRenderTextureType.BindableTexture, s_CopyId);
            }


            // Set properties for effect.
            cb.SetGlobalVector(s_EffectFactorId, new Vector4(request.effectFactor, 0));
            cb.SetGlobalVector(s_ColorFactorId, new Vector4(request.effectColor.r, request.effectColor.g, request.effectColor.b, request.colorFactor));

            // [2] Apply base effect with reduction buffer (copied screen -> effect1).
            var mat = GetMaterial(request);
            GetSamplingSize(request.reductionRate, out w, out h);
            cb.GetTemporaryRT(s_EffectId1, w, h, 0, request.filterMode);
            cb.Blit(s_CopyId, s_EffectId1, mat, 0);
            cb.ReleaseTemporaryRT(s_CopyId);

            // Iterate blurring operation.
            if (request.blurMode != UIEffectSnapshotRequest.BlurMode.None)
            {
                cb.GetTemporaryRT(s_EffectId2, w, h, 0, request.filterMode);
                for (var i = 0; i < request.blurIterations; i++)
                {
                    // [3] Apply blurring with reduction buffer (effect1 -> effect2, or effect2 -> effect1).
                    cb.SetGlobalVector(s_EffectFactorId, new Vector4(request.blurFactor, 0));
                    cb.Blit(s_EffectId1, s_EffectId2, mat, 1);
                    cb.SetGlobalVector(s_EffectFactorId, new Vector4(0, request.blurFactor));
                    cb.Blit(s_EffectId2, s_EffectId1, mat, 1);
                }

                cb.ReleaseTemporaryRT(s_EffectId2);
            }

            // [4] Copy to result RT.
            cb.Blit(s_EffectId1, new RenderTargetIdentifier(request.renderTexture));
            cb.ReleaseTemporaryRT(s_EffectId1);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Graphics.ExecuteCommandBuffer(cb);

                if (request.postAction != null)
                    request.postAction(request);
                return;
            }
#endif
            // Execute command buffer.
            instance.StartCoroutine(CoUpdateTextureOnNextFrame_Internal(request));
        }

        /// <summary>
        /// Set texture on next frame.
        /// </summary>
        private static IEnumerator CoUpdateTextureOnNextFrame_Internal(UIEffectSnapshotRequest request)
        {
            yield return new WaitForEndOfFrame();

#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            {
                // Execute command buffer.
                Graphics.ExecuteCommandBuffer(request.commandBuffer);
            }

            request.commandBuffer.Release();
            if (request.postAction != null)
                request.postAction(request);
        }
    }
}
