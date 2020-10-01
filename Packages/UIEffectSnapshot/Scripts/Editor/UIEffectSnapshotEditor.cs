using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UI;
using UnityEditorInternal;
using UnityEngine;
using EffectMode = Coffee.UIExtensions.UIEffectSnapshotRequest.EffectMode;
using ColorMode = Coffee.UIExtensions.UIEffectSnapshotRequest.ColorMode;
using BlurMode = Coffee.UIExtensions.UIEffectSnapshotRequest.BlurMode;

namespace Coffee.UIExtensions.Editors
{
    /// <summary>
    /// UIEffectCapturedImage editor.
    /// </summary>
    [CustomEditor(typeof(UIEffectSnapshot))]
    [CanEditMultipleObjects]
    internal class UIEffectSnapshotEditor : RawImageEditor
    {
        private readonly GUIContent _contentCaptureEffect = new GUIContent("Effect Settings On Capture");
        private readonly GUIContent _contentAdvancedOption = new GUIContent("Advanced Option");
        private readonly GUIContent _contentResultTextureSettings = new GUIContent("Result Texture Settings");
        private readonly GUIContent _contentDebug = new GUIContent("Debug");
        private readonly GUIContent _contentCapture = new GUIContent("Capture");
        private readonly GUIContent _contentRelease = new GUIContent("Release");
        private readonly GUIContent _contentCustomMaterial = new GUIContent("Custom Material For Effect");

        private SerializedProperty _spColor;
        private SerializedProperty _spRayCastTarget;
        private SerializedProperty _spMaskable;
        private SerializedProperty _spDownSamplingRate;
        private SerializedProperty _spReductionRate;
        private SerializedProperty _spFilterMode;
        private SerializedProperty _spBlurIterations;
        private SerializedProperty _spFitToScreen;
        private SerializedProperty _spCaptureOnEnable;
        private SerializedProperty _spGlobalMode;
        private SerializedProperty _spEffectMode;
        private SerializedProperty _spEffectFactor;
        private SerializedProperty _spColorMode;
        private SerializedProperty _spColorFactor;
        private SerializedProperty _spEffectColor;
        private SerializedProperty _spBlurMode;
        private SerializedProperty _spBlurFactor;
        private ReorderableList _ro;
        private readonly MaterialArrayEditor _materialArrayEditor = new MaterialArrayEditor();


        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            _spColor = serializedObject.FindProperty("m_Color");
            _spRayCastTarget = serializedObject.FindProperty("m_RaycastTarget");
            _spMaskable = serializedObject.FindProperty("m_Maskable");

            _spFitToScreen = serializedObject.FindProperty("m_FitToScreen");
            _spCaptureOnEnable = serializedObject.FindProperty("m_CaptureOnEnable");
            _spGlobalMode = serializedObject.FindProperty("m_GlobalMode");

            var r = serializedObject.FindProperty("m_Request");
            _spDownSamplingRate = r.FindPropertyRelative("m_SamplingRate");
            _spReductionRate = r.FindPropertyRelative("m_ReductionRate");
            _spFilterMode = r.FindPropertyRelative("m_FilterMode");
            _spEffectMode = r.FindPropertyRelative("m_EffectMode");
            _spEffectFactor = r.FindPropertyRelative("m_EffectFactor");
            _spColorMode = r.FindPropertyRelative("m_ColorMode");
            _spColorFactor = r.FindPropertyRelative("m_ColorFactor");
            _spEffectColor = r.FindPropertyRelative("m_EffectColor");
            _spBlurMode = r.FindPropertyRelative("m_BlurMode");
            _spBlurFactor = r.FindPropertyRelative("m_BlurFactor");
            _spDownSamplingRate = r.FindPropertyRelative("m_DownSamplingRate");
            _spReductionRate = r.FindPropertyRelative("m_ReductionRate");
            _spFilterMode = r.FindPropertyRelative("m_FilterMode");
            _spBlurIterations = r.FindPropertyRelative("m_BlurIterations");

            var sp = r.FindPropertyRelative("m_CustomMaterials");
            _ro = new ReorderableList(sp.serializedObject, sp, true, true, true, true);
            _ro.elementHeight = EditorGUIUtility.singleLineHeight + 4;
            _ro.drawElementCallback = (rect, index, active, focused) =>
            {
                rect.y += 1;
                rect.height = EditorGUIUtility.singleLineHeight;
                var p = sp.GetArrayElementAtIndex(index);
                if (p.objectReferenceValue)
                {
                    EditorGUI.PropertyField(rect, p, GUIContent.none);
                }
                else
                {
                    rect.width -= 40;
                    EditorGUI.PropertyField(rect, p, GUIContent.none);

                    rect.x += rect.width;
                    rect.width = 40;
                    if (GUI.Button(rect, "New"))
                    {
                        var path = AssetDatabase.GenerateUniqueAssetPath("Assets/UI-Effect-Snapshot-Extra.mat");
                        var fileName = Path.GetFileName(path);
                        path = EditorUtility.SaveFilePanel("Save a new extra effect material", "Assets", fileName, "mat");
                        if (string.IsNullOrEmpty(path)) return;
                        path = path.Replace('\\', '/').Replace(Application.dataPath, "Assets");
                        AssetDatabase.CopyAsset("Packages/com.coffee.ui-effect-snapshot/Prefabs/UI-Effect-Snapshot-Extra.mat", path);
                        p.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>(path);
                    }
                }
            };
            _ro.drawHeaderCallback += rect => EditorGUI.LabelField(rect, _contentCustomMaterial);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _materialArrayEditor.Release();
        }

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var current = target as UIEffectSnapshot;
            if (current == null) return;
            serializedObject.Update();

            //==== Basic properties ====
            EditorGUILayout.PropertyField(_spColor);
            EditorGUILayout.PropertyField(_spRayCastTarget);
            EditorGUILayout.PropertyField(_spMaskable);

            GUILayout.Space(10);
            EditorGUILayout.LabelField(_contentCaptureEffect, EditorStyles.boldLabel);

            //==== Effect setting ====
            // When effect is enable, show parameters.
            EditorGUILayout.PropertyField(_spEffectMode);
            if (_spEffectMode.intValue != (int) EffectMode.None)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_spEffectFactor);
                EditorGUI.indentLevel--;
            }

            //==== Color setting ====
            EditorGUILayout.PropertyField(_spColorMode);
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_spEffectColor);
                if (_spColorMode.intValue != (int) ColorMode.Multiply)
                    EditorGUILayout.PropertyField(_spColorFactor);
                EditorGUI.indentLevel--;
            }

            //==== Blur setting ====
            // When blur is enable, show parameters.
            EditorGUILayout.PropertyField(_spBlurMode);
            if (_spBlurMode.intValue != (int) BlurMode.None)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_spBlurFactor);
                EditorGUILayout.PropertyField(_spBlurIterations); // Blur iterations.
                EditorGUI.indentLevel--;
            }

            //==== Advanced options ====
            GUILayout.Space(10);
            EditorGUILayout.LabelField(_contentAdvancedOption, EditorStyles.boldLabel);
            _ro.DoLayoutList(); // Custom Materials.
            EditorGUILayout.PropertyField(_spGlobalMode); // Global Mode.
            EditorGUILayout.PropertyField(_spCaptureOnEnable); // Capture On Enable.
            EditorGUILayout.PropertyField(_spFitToScreen); // Fit To Screen.

            //==== Result texture settings ====
            GUILayout.Space(10);
            EditorGUILayout.LabelField(_contentResultTextureSettings, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_spFilterMode); // Filter Mode.
            DrawDownSamplingRate(_spReductionRate); // Reduction rate.
            DrawDownSamplingRate(_spDownSamplingRate); // Down sampling rate.

            serializedObject.ApplyModifiedProperties();

            // Debug.
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label(_contentDebug);

                if (GUILayout.Button(_contentCapture, "ButtonLeft"))
                {
                    current.Release();
                    EditorApplication.delayCall += current.Capture;
                }

                EditorGUI.BeginDisabledGroup(!current.capturedTexture);
                if (GUILayout.Button(_contentRelease, "ButtonRight"))
                {
                    current.Release();
                }

                EditorGUI.EndDisabledGroup();
            }

            // Draw custom materials.
            var materials = targets
                .OfType<UIEffectSnapshot>()
                .SelectMany(x => x.request.customMaterials)
                .Where(x => x)
                .ToArray();
            _materialArrayEditor.Draw(materials);
        }

        /// <summary>
        /// Draws the down sampling rate.
        /// </summary>
        private static void DrawDownSamplingRate(SerializedProperty sp)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(sp);
                int w, h;
                UIEffectSnapshotUpdater.GetSamplingSize((UIEffectSnapshotRequest.DownSamplingRate) sp.intValue, out w, out h);
                GUILayout.Label(string.Format("{0}x{1}", w, h), EditorStyles.miniLabel);
            }
        }
    }
}
