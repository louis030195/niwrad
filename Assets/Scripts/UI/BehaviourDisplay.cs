using System.Collections;
using AI;
using Evolution;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	[RequireComponent(typeof(MemeController))]
	public class BehaviourDisplay : MonoBehaviour
	{
		// [Header("Parameters")]
		[Header("Objects references")]
		public TextMeshProUGUI memeText;
		public Image memeBackgroundImage;

		private void Awake()
		{
			GetComponent<MemeController>().MemeChanged += UpdateUi;
		}

		private void UpdateUi(Meme value)
		{
			// TODO: display when acting etc ...
			memeText.text = $"{value.Name}";
			memeText.color = value.SceneGizmoColor * 0.6f; // Need contrast font / background
			memeBackgroundImage.color = value.SceneGizmoColor;
		}
	}
}
