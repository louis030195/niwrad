using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(TextureCreator))]
    public class TextureCreatorInspector : UnityEditor.Editor {

        private TextureCreator creator;

        private void OnEnable () {
            creator = target as TextureCreator;
            Undo.undoRedoPerformed += RefreshCreator;
        }

        private void OnDisable () {
            Undo.undoRedoPerformed -= RefreshCreator;
        }

        private void RefreshCreator () {
            if (Application.isPlaying) {
                creator.Fill();
            }
        }

        public override void OnInspectorGUI () {
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck()) {
                RefreshCreator();
            }
        }
    }
}
