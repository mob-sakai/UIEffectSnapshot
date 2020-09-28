using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using SamplingRate = Coffee.UIExtensions.UIEffectSnapshotRequest.SamplingRate;
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
    public class UIEffectSnapshotEditor : RawImageEditor
    {
        public enum QualityMode
        {
            Fast = (SamplingRate.x2 << 0) + (SamplingRate.x2 << 4) + (FilterMode.Bilinear << 8) + (2 << 10),
            Medium = (SamplingRate.x1 << 0) + (SamplingRate.x1 << 4) + (FilterMode.Bilinear << 8) + (3 << 10),
            Detail = (SamplingRate.None << 0) + (SamplingRate.x1 << 4) + (FilterMode.Bilinear << 8) + (5 << 10),
            Custom = -1,
        }

        private const int Bits4 = (1 << 4) - 1;
        private const int Bits2 = (1 << 2) - 1;

        private readonly GUIContent _contentCaptureEffect = new GUIContent("Capture Effect");
        private readonly GUIContent _contentAdvancedOption = new GUIContent("Advanced Option");
        private readonly GUIContent _contentQualityMode = new GUIContent("Quality Mode");
        private readonly GUIContent _contentResultTextureSetting = new GUIContent("Result Texture Setting");
        private readonly GUIContent _contentDebug = new GUIContent("Debug");
        private readonly GUIContent _contentCapture = new GUIContent("Capture");
        private readonly GUIContent _contentRelease = new GUIContent("Release");

        private bool _customAdvancedOption = false;
        private SerializedProperty _spTexture;
        private SerializedProperty _spColor;
        private SerializedProperty _spRayCastTarget;
        private SerializedProperty _spMaskable;
        private SerializedProperty _spSamplingRate;
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


        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            _spTexture = serializedObject.FindProperty("m_Texture");
            _spColor = serializedObject.FindProperty("m_Color");
            _spRayCastTarget = serializedObject.FindProperty("m_RaycastTarget");
            _spMaskable = serializedObject.FindProperty("m_Maskable");

            _spFitToScreen = serializedObject.FindProperty("m_FitToScreen");
            _spCaptureOnEnable = serializedObject.FindProperty("m_CaptureOnEnable");
            _spGlobalMode = serializedObject.FindProperty("m_GlobalMode");

            var r = serializedObject.FindProperty("m_Request");
            _spSamplingRate = r.FindPropertyRelative("m_SamplingRate");
            _spReductionRate = r.FindPropertyRelative("m_ReductionRate");
            _spFilterMode = r.FindPropertyRelative("m_FilterMode");
            _spEffectMode = r.FindPropertyRelative("m_EffectMode");
            _spEffectFactor = r.FindPropertyRelative("m_EffectFactor");
            _spColorMode = r.FindPropertyRelative("m_ColorMode");
            _spColorFactor = r.FindPropertyRelative("m_ColorFactor");
            _spEffectColor = r.FindPropertyRelative("m_EffectColor");
            _spBlurMode = r.FindPropertyRelative("m_BlurMode");
            _spBlurFactor = r.FindPropertyRelative("m_BlurFactor");
            _spSamplingRate = r.FindPropertyRelative("m_SamplingRate");
            _spReductionRate = r.FindPropertyRelative("m_ReductionRate");
            _spFilterMode = r.FindPropertyRelative("m_FilterMode");
            _spBlurIterations = r.FindPropertyRelative("m_BlurIterations");

            _customAdvancedOption = qualityMode == QualityMode.Custom;
        }

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var current = target as UIEffectSnapshot;
            if (current == null) return;
            serializedObject.Update();

            //================
            // Basic properties.
            //================
            EditorGUILayout.PropertyField(_spTexture);
            EditorGUILayout.PropertyField(_spColor);
            EditorGUILayout.PropertyField(_spRayCastTarget);
            EditorGUILayout.PropertyField(_spMaskable);

            GUILayout.Space(10);
            EditorGUILayout.LabelField(_contentCaptureEffect, EditorStyles.boldLabel);

            //================
            // Effect setting.
            //================
            // When effect is enable, show parameters.
            EditorGUILayout.PropertyField(_spEffectMode);
            if (_spEffectMode.intValue != (int) EffectMode.None)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_spEffectFactor);
                EditorGUI.indentLevel--;
            }

            //================
            // Color setting.
            //================
            EditorGUILayout.PropertyField(_spColorMode);
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_spEffectColor);
                if (_spColorMode.intValue != (int) ColorMode.Multiply)
                    EditorGUILayout.PropertyField(_spColorFactor);
                EditorGUI.indentLevel--;
            }

            //================
            // Blur setting.
            //================
            // When blur is enable, show parameters.
            EditorGUILayout.PropertyField(_spBlurMode);
            if (_spBlurMode.intValue != (int) BlurMode.None)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_spBlurFactor);
                EditorGUI.indentLevel--;
            }

            //================
            // Advanced options.
            //================
            GUILayout.Space(10);
            EditorGUILayout.LabelField(_contentAdvancedOption, EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_spGlobalMode); // Global Mode.
            EditorGUILayout.PropertyField(_spCaptureOnEnable); // Capture On Enable.
            EditorGUILayout.PropertyField(_spFitToScreen); // Fit To Screen.

            EditorGUI.BeginChangeCheck();
            var quality = qualityMode;
            quality = (QualityMode) EditorGUILayout.EnumPopup(_contentQualityMode, quality);
            if (EditorGUI.EndChangeCheck())
            {
                _customAdvancedOption = quality == QualityMode.Custom;
                qualityMode = quality;
            }

            // When qualityMode is `Custom`, show advanced option.
            if (_customAdvancedOption)
            {
                if ((BlurMode) _spBlurMode.intValue != BlurMode.None)
                {
                    EditorGUILayout.PropertyField(_spBlurIterations); // Blur iterations.
                }

                DrawSamplingRate(_spReductionRate); // Reduction rate.

                EditorGUILayout.Space();
                EditorGUILayout.LabelField(_contentResultTextureSetting, EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(_spFilterMode); // Filter Mode.
                DrawSamplingRate(_spSamplingRate); // Sampling rate.
            }

            serializedObject.ApplyModifiedProperties();

            // Debug.
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label(_contentDebug);

                if (GUILayout.Button(_contentCapture, "ButtonLeft"))
                {
                    current.Capture();
                }

                EditorGUI.BeginDisabledGroup(!current.capturedTexture);
                if (GUILayout.Button(_contentRelease, "ButtonRight"))
                {
                    current.Release();
                }

                EditorGUI.EndDisabledGroup();
            }
        }

        private QualityMode qualityMode
        {
            get
            {
                if (_customAdvancedOption)
                    return QualityMode.Custom;

                var qualityValue = (_spSamplingRate.intValue << 0)
                                   + (_spReductionRate.intValue << 4)
                                   + (_spFilterMode.intValue << 8)
                                   + (_spBlurIterations.intValue << 10);

                return System.Enum.IsDefined(typeof(QualityMode), qualityValue) ? (QualityMode) qualityValue : QualityMode.Custom;
            }
            set
            {
                if (value == QualityMode.Custom) return;

                var qualityValue = (int) value;
                _spSamplingRate.intValue = (qualityValue >> 0) & Bits4;
                _spReductionRate.intValue = (qualityValue >> 4) & Bits4;
                _spFilterMode.intValue = (qualityValue >> 8) & Bits2;
                _spBlurIterations.intValue = (qualityValue >> 10) & Bits4;
            }
        }

        /// <summary>
        /// Draws the Sampling rate.
        /// </summary>
        private static void DrawSamplingRate(SerializedProperty sp)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(sp);
                int w, h;
                UIEffectSnapshotProcesser.GetSamplingSize((SamplingRate) sp.intValue, out w, out h);
                GUILayout.Label(string.Format("{0}x{1}", w, h), EditorStyles.miniLabel);
            }
        }
    }
}
