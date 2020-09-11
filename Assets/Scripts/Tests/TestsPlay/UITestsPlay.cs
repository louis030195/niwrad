using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
            var canvas =
                Helper.RenderScriptableObject(
                    Resources.Load(Helper.TestScriptableObjectBasicAnimalCharacteristicsPath) as ScriptableObject);
            await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Escape));
            foreach (Transform t in canvas.transform)
            {
                Object.DestroyImmediate(t.gameObject);
            }
        });

        [UnityTest]
        public IEnumerator RenderScriptableObjectFromExperience() => UniTask.ToCoroutine(async () =>
        {
            Debug.Log($"Press escape to continue");
            var canvas =
                Helper.RenderScriptableObject(
                    Resources.Load(Helper.TestScriptableObjectBasicExperiencePath) as ScriptableObject);
            await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Escape));
            foreach (Transform t in canvas.transform)
            {
                Object.DestroyImmediate(t.gameObject);
            }
        });
    }
}
