using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Evolution;
using Tests.TestsHelpers;
using UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests.TestsPlay
{
    public class UITestsPlay
    {
        [UnityTest]
        public IEnumerator RenderScriptableObjectFromAnimalCharacteristics() => UniTask.ToCoroutine(async () =>
        {
            Debug.Log($"Press escape to continue");
            var go = new GameObject("AnimalCharacteristics");
            new AnimalCharacteristics().Render(go.transform);
            // TODO: assert stuff ...
            await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Escape));
            Object.DestroyImmediate(go);
        });

        [UnityTest]
        public IEnumerator RenderScriptableObjectFromExperience() => UniTask.ToCoroutine(async () =>
        {
            Debug.Log($"Press escape to continue");
            var go = new GameObject("Experience");
            new Experience().Render(go.transform);
            // TODO: assert stuff ...
            await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Escape));
            Object.DestroyImmediate(go);
        });
    }
}
