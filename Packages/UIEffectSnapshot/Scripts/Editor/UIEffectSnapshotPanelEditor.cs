using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Coffee.UIExtensions.Editors
{
    /// <summary>
    /// UIEffectCapturedImage editor.
    /// </summary>
    [CustomEditor(typeof(UIEffectSnapshotPanel))]
    [CanEditMultipleObjects]
    internal class UIEffectSnapshotPanelEditor : Editor
    {
        static GUIContent _contentHeader = new GUIContent("UI Effect Snapshots To Control");

        private SerializedProperty _spTransitionDuration;
        private SerializedProperty _spShowOnEnable;
        private SerializedProperty _spDeactivateOnHidden;
        private ReorderableList _ro;

        private void OnEnable()
        {
            _spTransitionDuration = serializedObject.FindProperty("m_TransitionDuration");
            _spShowOnEnable = serializedObject.FindProperty("m_ShowOnEnable");
            _spDeactivateOnHidden = serializedObject.FindProperty("m_DeactivateOnHidden");

            var sp = serializedObject.FindProperty("m_Snapshots");
            _ro = new ReorderableList(sp.serializedObject, sp, false, true, true, true);
            _ro.elementHeight = EditorGUIUtility.singleLineHeight + 4;
            _ro.drawElementCallback = (r, index, active, focused) =>
            {
                r = new Rect(r.x, r.y + 1, r.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(r, sp.GetArrayElementAtIndex(index), GUIContent.none);
            };
            _ro.drawHeaderCallback += rect =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 180, rect.height), _contentHeader);

                if (!GUI.Button(new Rect(rect.width - 40, rect.y - 1, 60, rect.height), "Refresh", EditorStyles.miniButton)) return;

                foreach (UIEffectSnapshotPanel t in targets)
                {
                    t.Refresh();
                }
            };
        }

        public override void OnInspectorGUI()
        {
            var current = target as UIEffectSnapshotPanel;
            if (current == null) return;

            serializedObject.Update();

            GUILayout.Space(4);
            _ro.DoLayoutList();
            EditorGUILayout.PropertyField(_spTransitionDuration);
            EditorGUILayout.PropertyField(_spShowOnEnable);
            EditorGUILayout.PropertyField(_spDeactivateOnHidden);

            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(10);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Debug");

                if (GUILayout.Button("Show", "ButtonLeft"))
                    current.Show();

                if (GUILayout.Button("Hide", "ButtonRight"))
                    current.Hide();
            }
        }
    }
}
