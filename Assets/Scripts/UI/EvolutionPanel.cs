using Api.Realtime;
using Evolution;
using TMPro;
using UnityEngine;

namespace UI
{
	public class EvolutionPanel : MonoBehaviour // TODO: spawn middle above terrain ?
	{
		[SerializeField]
		private TextMeshProUGUI animals;
		[SerializeField]
		private TextMeshProUGUI plants;

        private void Start()
        {
            Hm.instance.Statistics.Pushed += StatisticsOnPushed;
        }

        private void StatisticsOnPushed((ExperienceSample p, float t) obj)
        {
            animals.text = $"{obj.p.Animals}";
            plants.text = $"{obj.p.Plants}";
        }
    }
}
