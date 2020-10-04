using System;
using Cysharp.Threading.Tasks;
using Api.Match;
using Api.Realtime;
using Api.Session;
using Evolution;
using ProceduralTree;
using TMPro;
using UnityEngine;
using Utils;

namespace UI
{
	public class EvolutionPanel : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI animals;
		[SerializeField]
		private TextMeshProUGUI plants;

        private void Start()
        {
            Hm.instance.Statistics.Pushed += StatisticsOnPushed;
        }

        private void StatisticsOnPushed((TimeSeriePoint p, float t) obj)
        {
            animals.text = $"{obj.p.Animals}";
            plants.text = $"{obj.p.Plants}";
        }
    }
}
