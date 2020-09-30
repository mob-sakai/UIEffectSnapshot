using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIExtensions.Editors
{
    internal class MenuOptions_UIEffectSnapshot
    {
        [MenuItem("GameObject/UI/UI Effect Snapshot", false, 12000)]
        private static void CreateUIEffectSnapshot(MenuCommand menuCommand)
        {
            EditorApplication.ExecuteMenuItem("GameObject/UI/Image");
            var instance = Selection.activeGameObject;
            Object.DestroyImmediate(instance.GetComponent<Image>());

            instance.AddComponent<UIEffectSnapshot>();
            instance.name = ObjectNames.NicifyVariableName(typeof(UIEffectSnapshot).Name);
        }

        [MenuItem("GameObject/UI/UI Effect Snapshot Panel/With Blurred Background", false, 12011)]
        private static void CreateUIEffectSnapshotPanel_Bg(MenuCommand menuCommand)
        {
            CreateUIEffectSnapshotPanel(true, false);
        }

        [MenuItem("GameObject/UI/UI Effect Snapshot Panel/With Blurred Panel", false, 12012)]
        private static void CreateUIEffectSnapshotPanel_Panel(MenuCommand menuCommand)
        {
            CreateUIEffectSnapshotPanel(false, true);
        }

        [MenuItem("GameObject/UI/UI Effect Snapshot Panel/With Blurred Background & Panel", false, 12013)]
        private static void CreateUIEffectSnapshotPanel_BgPanel(MenuCommand menuCommand)
        {
            CreateUIEffectSnapshotPanel(true, true);
        }

        private static void CreateUIEffectSnapshotPanel(bool background, bool panel)
        {
            EditorApplication.ExecuteMenuItem("GameObject/UI/Image");
            var dummy = Selection.activeGameObject;
            var parent = dummy.transform.parent;
            Object.DestroyImmediate(dummy);

            var prefabPath = AssetDatabase.GUIDToAssetPath("c57e7abcda99a408f83df0eae2e3e8dd");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var instance = Object.Instantiate(prefab, parent);
            instance.name = ObjectNames.NicifyVariableName(typeof(UIEffectSnapshotPanel).Name);
            Selection.activeGameObject = instance;

            var snapShots = instance.GetComponentsInChildren<UIEffectSnapshot>();

            if (!background)
            {
                var go = snapShots[0].gameObject;
                Object.DestroyImmediate(snapShots[0]);
                var image = go.AddComponent<Image>();
                image.name = "Background";
                image.rectTransform.sizeDelta = Vector2.zero;
                image.color = new Color(1, 1, 1, 0.01f);
            }

            if (!panel)
            {
                var go = snapShots[1].gameObject;
                Object.DestroyImmediate(snapShots[1]);
                var image = go.AddComponent<Image>();
                image.name = "Panel";
                image.rectTransform.sizeDelta = Vector2.zero;
                image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
                image.color = new Color(1, 1, 1, 0.2f);
                image.type = Image.Type.Sliced;
            }

            instance.GetComponent<UIEffectSnapshotPanel>().Refresh();
        }
    }
}
