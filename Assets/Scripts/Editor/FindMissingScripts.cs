using UnityEngine;
using UnityEditor;

namespace Editor
{
    public class FindMissingScripts : EditorWindow
    {
        [MenuItem("Window/FindMissingScripts")]
        public static void ShowWindow()
        {
            GetWindow(typeof(FindMissingScripts));
        }

        public void OnGUI()
        {
            if (GUILayout.Button("Find Missing Scripts in selected prefabs"))
            {
                FindInSelected();
            }
        }

        private static void FindInSelected()
        {
            var go = Selection.gameObjects;
            int goCount = 0, componentsCount = 0, missingCount = 0;
            foreach (var g in go)
            {
                goCount++;
                var components = g.GetComponents<Component>();
                for (var i = 0; i < components.Length; i++)
                {
                    componentsCount++;
                    if (components[i] != null) continue;
                    missingCount++;
                    var s = g.name;
                    var t = g.transform;
                    while (t.parent != null)
                    {
                        s = t.parent.name + "/" + s;
                        t = t.parent;
                    }

                    Debug.Log(s + " has an empty script attached in position: " + i, g);
                }
            }

            Debug.Log($"Searched {goCount} GameObjects, {componentsCount} components, found {missingCount} missing");
        }
    }
}
