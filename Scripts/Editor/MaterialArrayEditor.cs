using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Coffee.UIExtensions.Editors
{
    internal class MaterialArrayEditor
    {
        private Editor[] _editors;

        public void Release()
        {
            if (_editors == null) return;
            foreach (var e in _editors)
            {
                if (e == null) continue;
                Object.DestroyImmediate(e);
            }

            _editors = null;
        }

        public void Draw(Material[] materials)
        {
            if (materials == null || materials.Length == 0)
            {
                Release();
                return;
            }

            if (_editors != null && !materials.SequenceEqual(_editors.Select(x => x.target)))
                Release();

            _editors = _editors ?? materials.Select(Editor.CreateEditor).ToArray();
            foreach (var e in _editors)
            {
                if (!e) continue;
                e.DrawHeader();
                e.OnInspectorGUI();
            }
        }
    }
}
