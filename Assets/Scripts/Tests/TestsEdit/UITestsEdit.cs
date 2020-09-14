using Evolution;
using NUnit.Framework;
using Tests.TestsHelpers;
using UnityEngine;

namespace Tests.TestsEdit
{
    public class UITestsEdit
    {
        [Test]
        public void RenderScriptableObjectFromAnimalCharacteristics()
        {
            var go = new GameObject("AnimalCharacteristics");
            new AnimalCharacteristics().Render(go.transform);
            // TODO: assert stuff ...
            Object.DestroyImmediate(go);
        }
        
        [Test]
        public void RenderScriptableObjectFromExperience() {
            var go = new GameObject("Experiences");
            new Experience().Render(go.transform);
            // TODO: assert stuff ...
            Object.DestroyImmediate(go);
        }
    }
}
