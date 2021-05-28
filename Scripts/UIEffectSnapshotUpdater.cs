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
    internal class UIEffectSnapshotUpdater : MonoBehaviour
    {
        static UIEffectSnapshotUpdater s_Instance;
        private readonly List<UIEffectSnapshotRequest> s_Requests = new List<UIEffectSnapshotRequest>();
        private static int s_CopyId;
        private static int s_EffectId1;
        private static int s_EffectId2;
        private static int s_EffectFactorId;
        private static int s_ColorFactorId;
        private static RenderTexture s_GlobalRt;
        private static Shader s_EffectShader;
        private static Dictionary<int,Material> s_MaterialMap = new Dictionary<int, Material>();


#if UNITY_2018_3_OR_NEWER && UNITY_EDITOR
        static UIEffectSnapshotUpdater s_InstanceForPrefab;

        private static UIEffectSnapshotUpdater InstanceForPrefab
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

        public static UIEffectSnapshotUpdater instance
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
                    : (s_Instance = FindObjectOfType<UIEffectSnapshotUpdater>() ?? Create());
            }
        }

        public RenderTexture globalCapturedTexture
        {
            get { return s_GlobalRt; }
        }

        private static UIEffectSnapshotUpdater Create()
        {
            var gameObject = new GameObject()
            {
                name = typeof(UIEffectSnapshotUpdater).Name,
                hideFlags = HideFlags.HideAndDontSave,
            };

#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            {
                DontDestroyOnLoad(gameObject);
            }
            var inst = gameObject.AddComponent<UIEffectSnapshotUpdater>();
            return inst;
        }

        public void Register(UIEffectSnapshotRequest request)
        {
            if (request == null || s_Requests.Contains(request)) return;

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
            var hash = request.materialHash;
            Material material;
            if(!s_MaterialMap.TryGetValue(hash, out material) || !material)
            {
                material = new Material(s_EffectShader);
                material.shaderKeywords = new object[] {request.effectMode, request.colorMode, request.blurMode}
                    .Where(x => 0 < (int) x)
                    .Select(x => x.ToString().ToUpper())
                    .ToArray();
                s_MaterialMap[hash] = material;
            }
            return material;
        }

        /// <summary>
        /// Gets the size of the Sampling.
        /// </summary>
        public static void GetSamplingSize(UIEffectSnapshotRequest.DownSamplingRate rate, out int w, out int h)
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
            if (Screen.fullScreenMode == FullScreenMode.Windowed)
            {
                w = Screen.width;
                h = Screen.height;
            }
            else
            {
                w = Screen.currentResolution.width;
                h = Screen.currentResolution.height;
            }

            if (rate == UIEffectSnapshotRequest.DownSamplingRate.None)
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
            GetSamplingSize(request.downSamplingRate, out w, out h);
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
                rt.autoGenerateMips = false;
                rt.wrapMode = TextureWrapMode.Clamp;

                if (request.globalMode)
                {
                    s_GlobalRt = rt;
                }
            }
            request.renderTexture = rt;

            // Setup command buffer.
            SetupCommandBuffer(request);
        }

        private static void SetupCommandBuffer(UIEffectSnapshotRequest request)
        {
            // [1] Capture from back buffer (back buffer -> copied screen).
            int w, h;
            GetSamplingSize(UIEffectSnapshotRequest.DownSamplingRate.None, out w, out h);

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

            // Setup command buffer.
            SetupCommandBuffer(request, cb);

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

        private static void SetupCommandBuffer(UIEffectSnapshotRequest request, CommandBuffer cb)
        {
            RenderTargetIdentifier src = 0;
            RenderTargetIdentifier dst = s_CopyId;
            RenderTargetIdentifier result = new RenderTargetIdentifier(request.renderTexture);

            int w, h;
            GetSamplingSize(request.reductionRate, out w, out h);
            cb.GetTemporaryRT(s_EffectId1, w, h, 0, request.filterMode);
            cb.GetTemporaryRT(s_EffectId2, w, h, 0, request.filterMode);

            // Set properties for effect.
            cb.SetGlobalVector(s_EffectFactorId, new Vector4(request.effectFactor, 0));
            cb.SetGlobalVector(s_ColorFactorId, new Vector4(request.effectColor.r, request.effectColor.g, request.effectColor.b, request.colorFactor));

            // [2] Apply base effect with reduction buffer (copied screen -> effect1).
            var mat = GetMaterial(request);
            Blit(cb, ref src, ref dst, mat, 0);

            // [3] Iterate blurring operation.
            if (request.blurMode != UIEffectSnapshotRequest.BlurMode.None)
            {
                for (var i = 0; i < request.blurIterations; i++)
                {
                    // [3.1] Apply blurring (horizontal).
                    cb.SetGlobalVector(s_EffectFactorId, new Vector4(request.blurFactor, 0));
                    Blit(cb, ref src, ref dst, mat, 1);

                    // [3.2] Apply blurring (vertical).
                    cb.SetGlobalVector(s_EffectFactorId, new Vector4(0, request.blurFactor));
                    Blit(cb, ref src, ref dst, mat, 1);
                }
            }

            // [4] Apply custom effects
            foreach (var customMaterial in request.customMaterials)
            {
                if (customMaterial)
                    Blit(cb, ref src, ref dst, customMaterial, 0);
            }

            // [5] Copy to result RT.
            cb.Blit(dst, result);
            cb.ReleaseTemporaryRT(s_CopyId);
            cb.ReleaseTemporaryRT(s_EffectId1);
            cb.ReleaseTemporaryRT(s_EffectId2);
        }

        private static void Blit(CommandBuffer cb, ref RenderTargetIdentifier src, ref RenderTargetIdentifier dst, Material mat, int pass)
        {
            if (dst == s_CopyId)
            {
                src = s_CopyId;
                dst = s_EffectId1;
            }
            else if (dst == s_EffectId1)
            {
                src = s_EffectId1;
                dst = s_EffectId2;
            }
            else
            {
                src = s_EffectId2;
                dst = s_EffectId1;
            }
            cb.Blit(src, dst, mat, pass);
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
                if (request.commandBuffer != null)
                    Graphics.ExecuteCommandBuffer(request.commandBuffer);
            }

            if (request.commandBuffer != null)
            {
                request.commandBuffer.Release();
                request.commandBuffer = null;
            }

            if (request.postAction != null)
                request.postAction(request);
        }
    }
}
