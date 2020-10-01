using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Coffee.UIExtensions.Editors
{
    internal class KeywordToggleScope :  GUI.Scope
    {
        public KeywordToggleScope(string label, Material[] materials, string keyword)
        {
            var keywordEnables = materials.Select(x => x.IsKeywordEnabled(keyword))
                .Distinct()
                .ToArray();
            var mixed = 2 == keywordEnables.Length;
            var enabledAll = keywordEnables.All(x => x);

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = mixed;
            EditorGUILayout.Space();
            enabledAll = EditorGUILayout.BeginToggleGroup(label, enabledAll);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var m in materials)
                {
                    if (enabledAll)
                        m.EnableKeyword(keyword);
                    else
                        m.DisableKeyword(keyword);
                }
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        }

        protected override void CloseScope()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndToggleGroup();
        }
    }

    internal class UIEffectSnapshotExtraShaderGUI : ShaderGUI
    {
        private MaterialEditor _editor;
        private MaterialProperty[] _properties;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            _editor = materialEditor;
            _properties = properties;

            var materials = materialEditor.targets
                .OfType<Material>()
                .ToArray();

            DrawProperty("_Scale");

            using (new KeywordToggleScope("Vignette", materials, "VIGNETTE"))
            {
                DrawProperty("_VignetteIntensity");
            }

            using (new KeywordToggleScope("Distortion", materials, "DISTORTION"))
            {
                DrawProperty("_DistortionIntensity");
            }

            using (new KeywordToggleScope("Noise", materials, "NOISE"))
            {
                DrawProperty("_NoiseIntensity");
            }

            using (new KeywordToggleScope("Scanning Line", materials, "SCANNING_LINE"))
            {
                DrawProperty("_ScanningLineFrequency");
                DrawProperty("_ScanningLineIntensity");
            }

            using (new KeywordToggleScope("RGB Shift", materials, "RGB_SHIFT"))
            {
                DrawProperty("_RgbShiftIntensity");
                DrawProperty("_RgbShiftOffsetX");
                DrawProperty("_RgbShiftOffsetY");
            }

            // base.OnGUI(materialEditor, properties);
        }

        private void DrawProperty(string name)
        {
            var p = FindProperty(name, _properties);
            if (p == null) return;

            _editor.ShaderProperty(p, p.displayName);
        }
    }
}
