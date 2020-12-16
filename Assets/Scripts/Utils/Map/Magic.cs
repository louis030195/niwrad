using MapMagic.Core;
using MapMagic.Nodes;
using UnityEngine;

namespace Utils.Map
{
    public class Magic : MonoBehaviour
    {
        public Graph mapMagicGraph;
        private MapMagicObject _magic;
        // Start is called before the first frame update
        private void Start()
        {
            _magic = gameObject.AddComponent<MapMagicObject>();
            _magic.graph = mapMagicGraph;
            _magic.tiles.generateInfinite = true;
            _magic.StartGenerate(true, false);
        }
    }
}
