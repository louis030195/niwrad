using NUnit.Framework;
using Tests.TestsHelpers;
using UnityEngine;

namespace Tests.TestsEdit
{
    public class UITestsEdit
    {
        [Test]
        public void RenderScriptableObjectFromAnimalCharacteristics() {
            var canvas = 
                Helper.RenderScriptableObject(Resources.Load(Helper.TestScriptableObjectBasicAnimalCharacteristicsPath) as ScriptableObject);
            // TODO: assert stuff ...
            foreach (Transform t in canvas.transform)
            {
                Object.DestroyImmediate(t.gameObject);
            }
        }
        
        [Test]
        public void RenderScriptableObjectFromExperience() {
            var canvas = 
                Helper.RenderScriptableObject(Resources.Load(Helper.TestScriptableObjectBasicExperiencePath) as ScriptableObject);
            // TODO: assert stuff ...
            foreach (Transform t in canvas.transform)
            {
                Object.DestroyImmediate(t.gameObject);
            }
        }
    }
}
