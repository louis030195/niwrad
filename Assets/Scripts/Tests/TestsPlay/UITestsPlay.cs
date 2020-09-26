using System.Collections;
using Cysharp.Threading.Tasks;
using Evolution;
using NUnit.Framework;
using Tests.TestsHelpers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace Tests.TestsPlay
{
    public class UITestsPlay
    {
        [UnityTest]
        public IEnumerator RenderExperience() => UniTask.ToCoroutine(async () =>
        {
            Debug.Log($"Press escape to continue");
            var go = new GameObject("AnimalCharacteristics");
            var e = ExperienceExtensions.Load($"Assets/Scripts/Tests/Data/BasicExperience.json", true);
            Assert.NotNull(e);
            Helper.RenderExperience(e);
            // TODO: assert stuff ...
            await UniTask.WaitUntil(() => Keyboard.current.escapeKey.wasPressedThisFrame);
            Object.DestroyImmediate(go);
        });
    }
}
