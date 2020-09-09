using System.Collections;
using Cysharp.Threading.Tasks;
using UI;
using UnityEngine;
using UnityEngine.TestTools;
using Utils;

namespace Tests.TestsPlay
{
    public class DebugTestsPlay
    {
        [UnityTest]
        public IEnumerator DrawRay() => UniTask.ToCoroutine(async () =>
        {
            Physics.Raycast(Vector3.zero, Vector3.up, out var hit, 100f);
            hit.DrawRay(Vector3.zero, Vector3.up * 100f, Color.magenta, Mathf.Infinity);
            await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Escape));
        });
    }
}
